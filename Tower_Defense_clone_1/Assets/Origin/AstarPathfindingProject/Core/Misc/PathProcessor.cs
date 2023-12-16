using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
#if NETFX_CORE
	using Thread = Pathfinding.WindowsStore.Thread;
#else
	using Thread = System.Threading.Thread;
#endif

	public class PathProcessor {
		public event System.Action<Path> OnPathPreSearch;
		public event System.Action<Path> OnPathPostSearch;
		public event System.Action OnQueueUnblocked;

		// 스레드 제어 큐
		internal readonly ThreadControlQueue queue;

		// A* 경로 찾기 객체 참조
		readonly AstarPath astar;

		// 경로 반환 큐
		readonly PathReturnQueue returnQueue;

		// 경로 핸들러 배열
		readonly PathHandler[] pathHandlers;

		// 경로 찾기 스레드 배열
		readonly Thread[] threads;

		/// <summary>
		/// 멀티스레딩을 사용하지 않을 때에 사용되는 IEnumerator입니다.
		/// 멀티스레딩을 사용하지 않을 때, 별도의 코루틴을 사용합니다. 이 코루틴은 직접 StartCoroutine을 사용하는 것이 아니라,
		/// 별도의 함수가 메인 IEnumerator을 증가시키는 while 루프를 가지고 있습니다.
		/// 이렇게 하면 다른 함수가 언제든 스레드를 전진시킬 수 있으며 Unity가 업데이트되기를 기다리지 않아도 됩니다.
		/// See: CalculatePaths
		/// See: CalculatePathsHandler
		/// </summary>
		IEnumerator threadCoroutine;

		/// <summary>
		/// 이전 노드에 사용되지 않은 다음 노드 인덱스를 보유합니다.
		/// See: nodeIndexPool
		/// </summary>
		int nextNodeIndex = 1;

		/// <summary>
		/// 파괴된 노드에 대한 인덱스를 보유합니다.
		/// 노드가 자주 삭제되고 생성될 때 메모리 구조를 많이 낭비하지 않기 위해 노드 인덱스가 재사용됩니다.
		/// </summary>
		readonly Stack<int> nodeIndexPool = new Stack<int>();

		readonly List<int> locks = new List<int>();
		int nextLockID = 0;

#if UNITY_2017_3_OR_NEWER
		CustomSampler profilingSampler;
#endif

		/// <summary>
		/// 병렬 경로 탐색기 수입니다.
		/// 한 번에 경로를 계산할 수 있는 동시 프로세스 수를 반환합니다.
		/// 멀티스레딩을 사용하는 경우, 이 값은 스레드 수일 것이며, 멀티스레딩을 사용하지 않으면 항상 1입니다 (코루틴만 사용하기 때문입니다).
		/// See: threadInfos
		/// See: IsUsingMultithreading
		/// </summary>
		public int NumThreads {
			get {
				return pathHandlers.Length;
			}
		}

		/// <summary>멀티스레딩을 사용하는지 여부를 반환합니다.</summary>
		public bool IsUsingMultithreading
		{
			get
			{
				return threads != null;
			}
		}

		internal PathProcessor (AstarPath astar, PathReturnQueue returnQueue, int processors, bool multithreaded) {
			this.astar = astar;
			this.returnQueue = returnQueue;

			if (processors < 0) {
				throw new System.ArgumentOutOfRangeException("processors");
			}

			if (!multithreaded && processors != 1) {
				throw new System.Exception("Only a single non-multithreaded processor is allowed");
			}

			// Set up path queue with the specified number of receivers
			queue = new ThreadControlQueue(processors);
			pathHandlers = new PathHandler[processors];

			for (int i = 0; i < processors; i++) {
				pathHandlers[i] = new PathHandler(i, processors);
			}

			if (multithreaded) {
#if UNITY_2017_3_OR_NEWER
				profilingSampler = CustomSampler.Create("Calculating Path");
#endif

				threads = new Thread[processors];

				// Start lots of threads
				for (int i = 0; i < processors; i++) {
					var pathHandler = pathHandlers[i];
					threads[i] = new Thread(() => CalculatePathsThreaded(pathHandler));
#if !UNITY_SWITCH || UNITY_EDITOR
					// Note: Setting the thread name seems to crash when deploying for Switch: https://forum.arongranberg.com/t/path-processor-crashing-nintendo-switch-build/6584
					threads[i].Name = "Pathfinding Thread " + i;
#endif
					threads[i].IsBackground = true;
					threads[i].Start();
				}
			} else {
				// 멀티스레딩을 사용하지 않을 때 코루틴 시작
				threadCoroutine = CalculatePaths(pathHandlers[0]);
			}
		}

		/// <summary>경로 탐색 중에 실행을 방지합니다.</summary>
		public struct GraphUpdateLock {
			PathProcessor pathProcessor;
			int id;

			public GraphUpdateLock (PathProcessor pathProcessor, bool block) {
				this.pathProcessor = pathProcessor;
				id = pathProcessor.Lock(block);
			}

			/// <summary>
			/// 이 잠금이 경로 찾기 스레드가 더 많은 경로를 처리하지 못하게 하는 동안 true입니다.
			/// 참고: 이 잠금이 PausePathfinding(false)를 사용하여 획득된 경우에는 아직 경로 찾기 스레드가 일시 중지되지 않을 수 있습니다.
			/// </summary>
			public bool Held {
				get {
					return pathProcessor != null && pathProcessor.locks.Contains(id);
				}
			}

			/// <summary>다른 잠금이 여전히 유지되고 있지 않은 경우 경로 찾기를 다시 시작할 수 있도록 허용합니다.</summary>
			public void Release()
			{
				pathProcessor.Unlock(id);
			}
		}

		int Lock (bool block) {
			queue.Block();

			if (block) {
				while (!queue.AllReceiversBlocked) {
					if (IsUsingMultithreading) {
						Thread.Sleep(1);
					} else {
						TickNonMultithreaded();
					}
				}
			}

			nextLockID++;
			locks.Add(nextLockID);
			return nextLockID;
		}

		void Unlock (int id) {
			if (!locks.Remove(id)) {
				throw new System.ArgumentException("This lock has already been released");
			}

			// 활성 잠금이 더 이상 없는지 확인
			if (locks.Count == 0) {
				if (OnQueueUnblocked != null) OnQueueUnblocked();

				queue.Unblock();
			}
		}

		/// <summary>
		/// 경로 찾기 스레드가 새로운 경로를 계산하는 것을 방지합니다.
		///
		/// 반환: 잠금 개체입니다. 경로 찾기를 다시 시작하려면 이 개체에서 Unlock을 호출해야 합니다.
		///
		/// 참고: 대부분의 경우 사용자 코드에서 이를 호출해서는 안 됩니다.
		/// </summary>
		/// <param name="block">true인 경우 이 호출은 모든 경로 찾기 스레드가 일시 중지될 때까지 차단됩니다.
		/// 그렇지 않으면 스레드는 현재 수행 중인 작업이 완료되면 일시 중지됩니다.</param>
		public GraphUpdateLock PausePathfinding (bool block) {
			return new GraphUpdateLock(this, block);
		}

		public void TickNonMultithreaded () {
			// 경로 처리
			if (threadCoroutine != null) {
				try {
					threadCoroutine.MoveNext();
				} catch (System.Exception e) {
					// 이것은 경로 찾기를 종료할 것입니다
					threadCoroutine = null;

					// 큐 종료 예외는 무시해야 합니다. 이것들은 스레드를 종료하는 것이 목적입니다
					if (!(e is ThreadControlQueue.QueueTerminationException))
					{
						Debug.LogException(e);
						Debug.LogError("Unhandled exception during pathfinding. Terminating.");
						queue.TerminateReceivers();

						// 이것은 스레드를 종료하기 위해 예외를 던집니다
						try
						{
							queue.PopNoBlock(false);
						} catch {}
					}
				}
			}
		}

		/// <summary>각 스레드에서 'Join'을 호출하여 완료될 때까지 차단합니다.</summary>
		public void JoinThreads()
		{
			if (threads != null) {
				for (int i = 0; i < threads.Length; i++) {
					if (!threads[i].Join(200)) {
						Debug.LogError("Could not terminate pathfinding thread["+i+"] in 200ms, trying Thread.Abort");
						threads[i].Abort();
					}
				}
			}
		}

		/// <summary>각 스레드에서 'Abort'을 호출합니다</summary>
		public void AbortThreads () {
			if (threads == null) return;
			for (int i = 0; i < threads.Length; i++) {
				if (threads[i] != null && threads[i].IsAlive) threads[i].Abort();
			}
		}

		/// <summary>
		/// 새로운 글로벌 노드 인덱스를 반환합니다.
		/// 경고: 이 메서드는 직접 호출해서는 안 됩니다. 그래프 노드 생성자에서 사용됩니다.
		/// </summary>
		public int GetNewNodeIndex () {
			return nodeIndexPool.Count > 0 ? nodeIndexPool.Pop() : nextNodeIndex++;
		}

		/// <summary>
		/// 노드에 대한 임시 경로 데이터를 초기화합니다.
		/// 경고: 이 메서드는 직접 호출해서는 안 됩니다. 그래프 노드 생성자에서 사용됩니다.
		/// </summary>
		public void InitializeNode (GraphNode node) {
			if (!queue.AllReceiversBlocked) {
				throw new System.Exception("Trying to initialize a node when it is not safe to initialize any nodes. Must be done during a graph update. See http://arongranberg.com/astar/docs/graph-updates.php#direct");
			}

			for (int i = 0; i < pathHandlers.Length; i++) {
				pathHandlers[i].InitializeNode(node);
			}

			astar.hierarchicalGraph.OnCreatedNode(node);
		}

		/// <summary>
		/// 주어진 노드를 파괴합니다.
		/// 이 메서드는 노드가 그래프에서 연결이 해제된 후에 호출되어야 하므로 다른 노드에서 더 이상 접근할 수 없습니다.
		/// 그래프 업데이트 중에만 호출되어야 하며, 이 때 경로 찾기 스레드가 실행되지 않거나 일시 중지되어야 합니다.
		///
		/// 경고: 이 메서드는 사용자 코드에서 직접 호출해서는 안 됩니다. 내부적으로 시스템에서 사용됩니다.
		/// </summary>
		public void DestroyNode (GraphNode node) {
			if (node.NodeIndex == -1) return;

			nodeIndexPool.Push(node.NodeIndex);

			for (int i = 0; i < pathHandlers.Length; i++) {
				pathHandlers[i].DestroyNode(node);
			}

			astar.hierarchicalGraph.AddDirtyNode(node);
		}

		/// <summary>
		/// 주 경로 탐색 메서드 (다중 스레드).
		/// 이 메서드는 다중 스레딩이 활성화된 경우 경로 찾기 대기열에서 경로를 계산합니다.
		///
		/// 참고: CalculatePaths
		/// 참고: StartPath
		/// </summary>
		void CalculatePathsThreaded (PathHandler pathHandler) {
#if UNITY_2017_3_OR_NEWER
			UnityEngine.Profiling.Profiler.BeginThreadProfiling("Pathfinding", "Pathfinding thread #" + (pathHandler.threadID+1));
#endif

#if !ASTAR_FAST_BUT_NO_EXCEPTIONS
			try {
#endif

				// 한 번 실행하는 데 허용된 최대 틱 수입니다.
				// 한 틱은 밀리초의 1/10000입니다.
				// 스레드가 중지해야 하는지 주기적으로 확인해야 합니다.
				long maxTicks = (long)(10*10000);
				long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				while (true) 
				{
					// 현재 계산 중인 경로입니다
					Path path = queue.Pop();
#if UNITY_2017_3_OR_NEWER
					profilingSampler.Begin();
#endif
					// 내부 구현 메서드에 액세스
					IPathInternals ipath = (IPathInternals)path;

					// 더 많은 스레드를 허용하지 않도록 간단한 수정을 방지하려고 시도합니다.
					if (pathHandler.threadID > 0)
					{
						throw new System.Exception("Thread Error");
					}

					AstarProfiler.StartFastProfile(0);
					ipath.PrepareBase(pathHandler);

					// 이제 경로 처리 중
					// 처리 중 상태로 진행
					ipath.AdvanceState(PathState.Processing);

					// 일부 콜백 호출
					if (OnPathPreSearch != null) 
					{
					OnPathPreSearch(path);
					}

					// 경로 계산이 시작된 시간을 나타내는 틱
					long startTicks = System.DateTime.UtcNow.Ticks;

					// 경로 준비
					ipath.Prepare();

					AstarProfiler.EndFastProfile(0);

				if (path.CompleteState == PathCompleteState.NotCalculated) 
					{
						// 시각적 표시를 위해 마지막으로 계산된 경로를 설정하여 에디터(씬 뷰)에서 디버그 정보를 볼 수 있도록 함
						astar.debugPathData = ipath.PathHandler;
						astar.debugPathID = path.pathID;

						AstarProfiler.StartFastProfile(1);

						// 경로 초기화, 이제 검색을 시작할 준비가 됨
						ipath.Initialize();

						AstarProfiler.EndFastProfile(1);

						// 경로가 완전히 계산되지 않은 동안 루프 실행
						while (path.CompleteState == PathCompleteState.NotCalculated) {
							// 경로 계산에 대한 일부 작업 수행
							// 함수는 시간이 너무 많이 걸리면 반환하거나 계산이 완료되면 반환
							AstarProfiler.StartFastProfile(2);
							ipath.CalculateStep(targetTick);
							AstarProfiler.EndFastProfile(2);

							targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

							// 큐가 종료되어 더 이상 경로를 받아들이지 않아야 하는 경우 함수(따라서 스레드도)를 취소합니다.
							// 이것은 A* 객체가 파괴되려고 할 때 수행됩니다.
							// 경로가 반환되고 이 함수가 종료됩니다.
							if (queue.IsTerminating)
							{
								path.FailWithError("AstarPath object destroyed");
						}
					}

					path.duration = (System.DateTime.UtcNow.Ticks - startTicks)*0.0001F;

#if ProfileAstar
					System.Threading.Interlocked.Increment(ref AstarPath.PathsCompleted);
					System.Threading.Interlocked.Add(ref AstarPath.TotalSearchTime, System.DateTime.UtcNow.Ticks - startTicks);
#endif
				}

					// 노드 태깅 및 기타 정리
					ipath.Cleanup();

					AstarProfiler.StartFastProfile(9);

					if (path.immediateCallback != null) path.immediateCallback(path);

					if (OnPathPostSearch != null) 
					{
						OnPathPostSearch(path);
					}

					// 경로를 반환 스택에 푸시합니다
					// 주 메인 Unity 스레드에서 감지되고 최대한 빨리 반환됩니다(다음 지연 업데이트에서 희망적으로)
					returnQueue.Enqueue(path);

					// ReturnQueue로 진행됨
					ipath.AdvanceState(PathState.ReturnQueue);

				AstarProfiler.EndFastProfile(9);
#if UNITY_2017_3_OR_NEWER
				profilingSampler.End();
#endif
			}
#if !ASTAR_FAST_BUT_NO_EXCEPTIONS
		}
		catch (System.Exception e) {
#if !NETFX_CORE
			if (e is ThreadAbortException || e is ThreadControlQueue.QueueTerminationException)
#else
			if (e is ThreadControlQueue.QueueTerminationException)
#endif
			{
				if (astar.logPathResults == PathLog.Heavy)
					Debug.LogWarning("Shutting down pathfinding thread #" + pathHandler.threadID);
				return;
			}
			Debug.LogException(e);
			Debug.LogError("Unhandled exception during pathfinding. Terminating.");
				// 처리되지 않은 예외, 경로 찾기 중지
				queue.TerminateReceivers();
		} finally {
#if UNITY_2017_3_OR_NEWER
			UnityEngine.Profiling.Profiler.EndThreadProfiling();
#endif
		}
#endif

			Debug.LogError("Error : This part should never be reached.");
			queue.ReceiverTerminated();
		}

		/// <summary>
		/// 주 경로 탐색 메서드입니다.
		/// 이 메서드는 경로 탐색 대기열에서 경로를 계산합니다.
		///
		/// 참조: CalculatePathsThreaded
		/// 참조: StartPath
		/// </summary>
		IEnumerator CalculatePaths(PathHandler pathHandler)
		{
			// 코루틴 내에서 최대 틱 수
			long maxTicks = (long)(astar.maxFrameTime*10000);
			long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

			while (true) {
				// 현재 계산 중인 경로
				Path p = null;

				AstarProfiler.StartProfile("Path Queue");

				// 다음 계산할 경로 가져오기 시도
				bool blockedBefore = false;
				while (p == null) {
					try {
						p = queue.PopNoBlock(blockedBefore);
						blockedBefore |= p == null;
					} catch (ThreadControlQueue.QueueTerminationException) {
						yield break;
					}

					if (p == null) {
						AstarProfiler.EndProfile();
						yield return null;
						AstarProfiler.StartProfile("Path Queue");
					}
				}

				AstarProfiler.EndProfile();

				AstarProfiler.StartProfile("Path Calc");

				IPathInternals ip = (IPathInternals)p;

				// 한 번 실행할 수 있는 최대 틱 수
				// 한 틱은 밀리초의 1/10000입니다.
				maxTicks = (long)(astar.maxFrameTime * 10000);

				ip.PrepareBase(pathHandler);

				// 이제 경로 처리 중
				// 처리 중 상태로 전환
				ip.AdvanceState(PathState.Processing);

				// 몇몇 콜백 호출
				// 경쟁 조건을 피하기 위해 로컬 변수에 저장되어야 합니다.
				var tmpOnPathPreSearch = OnPathPreSearch;
				if (tmpOnPathPreSearch != null) tmpOnPathPreSearch(p);

				// 경로 계산이 시작된 틱 - 계산 시간 측정에 사용됨
				long startTicks = System.DateTime.UtcNow.Ticks;
				long totalTicks = 0;

				AstarProfiler.StartFastProfile(8);

				AstarProfiler.StartFastProfile(0);
				// 경로 준비
				AstarProfiler.StartProfile("경로 준비");
				ip.Prepare();
				AstarProfiler.EndProfile("경로 준비");
				AstarProfiler.EndFastProfile(0);

				// Prepare 호출로 경로가 완료된 경우 확인
				// 이렇게 되면 일반적으로 경로가 실패합니다
				if (p.CompleteState == PathCompleteState.NotCalculated)
				{
					// 디버그용으로 마지막으로 계산된 경로를 p로 설정하여 에디터(씬 뷰)에서 디버그 정보를 볼 수 있습니다.
					astar.debugPathData = ip.PathHandler;
					astar.debugPathID = p.pathID;

					// 경로 초기화, 이제 검색을 시작할 준비가 됨
					AstarProfiler.StartProfile("경로 초기화");
					ip.Initialize();
					AstarProfiler.EndProfile();

					// Init 함수에서 오류 발생 가능성
					while (p.CompleteState == PathCompleteState.NotCalculated)
					{
						// 경로 계산에 대한 일부 작업 수행
						// 함수는 시간이 오래 걸리거나 계산이 완료될 때 반환됩니다
						AstarProfiler.StartFastProfile(2);

						AstarProfiler.StartProfile("경로 계산 단계");
						ip.CalculateStep(targetTick);
						AstarProfiler.EndFastProfile(2);

						AstarProfiler.EndProfile();

						// 경로 계산이 완료되면 여기에서 직접 중단하는 대신에 잠깐 쉽니다.
						// 대기 시간을 줄이는 데 도움이 됩니다.
						if (p.CompleteState != PathCompleteState.NotCalculated) break;

						AstarProfiler.EndFastProfile(8);
						totalTicks += System.DateTime.UtcNow.Ticks - startTicks;
						// 다른 스레드가 작업할 수 있도록 양보/대기

						AstarProfiler.EndProfile();
						yield return null;
						AstarProfiler.StartProfile("경로 계산");

						startTicks = System.DateTime.UtcNow.Ticks;
						AstarProfiler.StartFastProfile(8);

						// 대기열이 종료되어 더 이상 경로를 받아들이지 않아야 하는 경우 취소 함수(따라서 스레드도) 호출
						// 이것은 A* 객체가 파괴될 때 수행됩니다
						// 경로는 반환되고 이 함수는 종료됩니다 (함수의 상단에 있는 유사한 IF 문 참조)
						if (queue.IsTerminating)
						{
							p.FailWithError("AstarPath 객체가 파괴됨");
						}

						targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
					}

					totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
					p.duration = totalTicks*0.0001F;

#if ProfileAstar
					System.Threading.Interlocked.Increment(ref AstarPath.PathsCompleted);
#endif
				}

				// 노드 태그 지우기 및 기타 정리
				ip.Cleanup();

				AstarProfiler.EndFastProfile(8);

				// 즉시 콜백 호출
				// 경쟁 조건을 피하기 위해 로컬 변수에 저장되어야 합니다.
				var tmpImmediateCallback = p.immediateCallback;
				if (tmpImmediateCallback != null) tmpImmediateCallback(p);

				AstarProfiler.StartFastProfile(13);

				// 경쟁 조건을 피하기 위해 로컬 변수에 저장되어야 합니다.
				var tmpOnPathPostSearch = OnPathPostSearch;
				if (tmpOnPathPostSearch != null) tmpOnPathPostSearch(p);

				AstarProfiler.EndFastProfile(13);

				// 경로를 반환 스택에 푸시
				// 경로는 주 Unity 스레드에서 감지되고 가능한 빨리 반환될 것입니다 (다음 지연 업데이트)
				returnQueue.Enqueue(p);

				ip.AdvanceState(PathState.ReturnQueue);

				AstarProfiler.EndProfile();

				// 많은 경로를 계산한 경우 잠시 기다립니다
				if (System.DateTime.UtcNow.Ticks > targetTick) {
					yield return null;
					targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				}
			}
		}
	}
}
