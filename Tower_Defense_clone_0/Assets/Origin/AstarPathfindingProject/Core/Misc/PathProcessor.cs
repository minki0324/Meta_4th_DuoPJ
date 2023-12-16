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

		// ������ ���� ť
		internal readonly ThreadControlQueue queue;

		// A* ��� ã�� ��ü ����
		readonly AstarPath astar;

		// ��� ��ȯ ť
		readonly PathReturnQueue returnQueue;

		// ��� �ڵ鷯 �迭
		readonly PathHandler[] pathHandlers;

		// ��� ã�� ������ �迭
		readonly Thread[] threads;

		/// <summary>
		/// ��Ƽ�������� ������� ���� ���� ���Ǵ� IEnumerator�Դϴ�.
		/// ��Ƽ�������� ������� ���� ��, ������ �ڷ�ƾ�� ����մϴ�. �� �ڷ�ƾ�� ���� StartCoroutine�� ����ϴ� ���� �ƴ϶�,
		/// ������ �Լ��� ���� IEnumerator�� ������Ű�� while ������ ������ �ֽ��ϴ�.
		/// �̷��� �ϸ� �ٸ� �Լ��� ������ �����带 ������ų �� ������ Unity�� ������Ʈ�Ǳ⸦ ��ٸ��� �ʾƵ� �˴ϴ�.
		/// See: CalculatePaths
		/// See: CalculatePathsHandler
		/// </summary>
		IEnumerator threadCoroutine;

		/// <summary>
		/// ���� ��忡 ������ ���� ���� ��� �ε����� �����մϴ�.
		/// See: nodeIndexPool
		/// </summary>
		int nextNodeIndex = 1;

		/// <summary>
		/// �ı��� ��忡 ���� �ε����� �����մϴ�.
		/// ��尡 ���� �����ǰ� ������ �� �޸� ������ ���� �������� �ʱ� ���� ��� �ε����� ����˴ϴ�.
		/// </summary>
		readonly Stack<int> nodeIndexPool = new Stack<int>();

		readonly List<int> locks = new List<int>();
		int nextLockID = 0;

#if UNITY_2017_3_OR_NEWER
		CustomSampler profilingSampler;
#endif

		/// <summary>
		/// ���� ��� Ž���� ���Դϴ�.
		/// �� ���� ��θ� ����� �� �ִ� ���� ���μ��� ���� ��ȯ�մϴ�.
		/// ��Ƽ�������� ����ϴ� ���, �� ���� ������ ���� ���̸�, ��Ƽ�������� ������� ������ �׻� 1�Դϴ� (�ڷ�ƾ�� ����ϱ� �����Դϴ�).
		/// See: threadInfos
		/// See: IsUsingMultithreading
		/// </summary>
		public int NumThreads {
			get {
				return pathHandlers.Length;
			}
		}

		/// <summary>��Ƽ�������� ����ϴ��� ���θ� ��ȯ�մϴ�.</summary>
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
				// ��Ƽ�������� ������� ���� �� �ڷ�ƾ ����
				threadCoroutine = CalculatePaths(pathHandlers[0]);
			}
		}

		/// <summary>��� Ž�� �߿� ������ �����մϴ�.</summary>
		public struct GraphUpdateLock {
			PathProcessor pathProcessor;
			int id;

			public GraphUpdateLock (PathProcessor pathProcessor, bool block) {
				this.pathProcessor = pathProcessor;
				id = pathProcessor.Lock(block);
			}

			/// <summary>
			/// �� ����� ��� ã�� �����尡 �� ���� ��θ� ó������ ���ϰ� �ϴ� ���� true�Դϴ�.
			/// ����: �� ����� PausePathfinding(false)�� ����Ͽ� ȹ��� ��쿡�� ���� ��� ã�� �����尡 �Ͻ� �������� ���� �� �ֽ��ϴ�.
			/// </summary>
			public bool Held {
				get {
					return pathProcessor != null && pathProcessor.locks.Contains(id);
				}
			}

			/// <summary>�ٸ� ����� ������ �����ǰ� ���� ���� ��� ��� ã�⸦ �ٽ� ������ �� �ֵ��� ����մϴ�.</summary>
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

			// Ȱ�� ����� �� �̻� ������ Ȯ��
			if (locks.Count == 0) {
				if (OnQueueUnblocked != null) OnQueueUnblocked();

				queue.Unblock();
			}
		}

		/// <summary>
		/// ��� ã�� �����尡 ���ο� ��θ� ����ϴ� ���� �����մϴ�.
		///
		/// ��ȯ: ��� ��ü�Դϴ�. ��� ã�⸦ �ٽ� �����Ϸ��� �� ��ü���� Unlock�� ȣ���ؾ� �մϴ�.
		///
		/// ����: ��κ��� ��� ����� �ڵ忡�� �̸� ȣ���ؼ��� �� �˴ϴ�.
		/// </summary>
		/// <param name="block">true�� ��� �� ȣ���� ��� ��� ã�� �����尡 �Ͻ� ������ ������ ���ܵ˴ϴ�.
		/// �׷��� ������ ������� ���� ���� ���� �۾��� �Ϸ�Ǹ� �Ͻ� �����˴ϴ�.</param>
		public GraphUpdateLock PausePathfinding (bool block) {
			return new GraphUpdateLock(this, block);
		}

		public void TickNonMultithreaded () {
			// ��� ó��
			if (threadCoroutine != null) {
				try {
					threadCoroutine.MoveNext();
				} catch (System.Exception e) {
					// �̰��� ��� ã�⸦ ������ ���Դϴ�
					threadCoroutine = null;

					// ť ���� ���ܴ� �����ؾ� �մϴ�. �̰͵��� �����带 �����ϴ� ���� �����Դϴ�
					if (!(e is ThreadControlQueue.QueueTerminationException))
					{
						Debug.LogException(e);
						Debug.LogError("Unhandled exception during pathfinding. Terminating.");
						queue.TerminateReceivers();

						// �̰��� �����带 �����ϱ� ���� ���ܸ� �����ϴ�
						try
						{
							queue.PopNoBlock(false);
						} catch {}
					}
				}
			}
		}

		/// <summary>�� �����忡�� 'Join'�� ȣ���Ͽ� �Ϸ�� ������ �����մϴ�.</summary>
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

		/// <summary>�� �����忡�� 'Abort'�� ȣ���մϴ�</summary>
		public void AbortThreads () {
			if (threads == null) return;
			for (int i = 0; i < threads.Length; i++) {
				if (threads[i] != null && threads[i].IsAlive) threads[i].Abort();
			}
		}

		/// <summary>
		/// ���ο� �۷ι� ��� �ε����� ��ȯ�մϴ�.
		/// ���: �� �޼���� ���� ȣ���ؼ��� �� �˴ϴ�. �׷��� ��� �����ڿ��� ���˴ϴ�.
		/// </summary>
		public int GetNewNodeIndex () {
			return nodeIndexPool.Count > 0 ? nodeIndexPool.Pop() : nextNodeIndex++;
		}

		/// <summary>
		/// ��忡 ���� �ӽ� ��� �����͸� �ʱ�ȭ�մϴ�.
		/// ���: �� �޼���� ���� ȣ���ؼ��� �� �˴ϴ�. �׷��� ��� �����ڿ��� ���˴ϴ�.
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
		/// �־��� ��带 �ı��մϴ�.
		/// �� �޼���� ��尡 �׷������� ������ ������ �Ŀ� ȣ��Ǿ�� �ϹǷ� �ٸ� ��忡�� �� �̻� ������ �� �����ϴ�.
		/// �׷��� ������Ʈ �߿��� ȣ��Ǿ�� �ϸ�, �� �� ��� ã�� �����尡 ������� �ʰų� �Ͻ� �����Ǿ�� �մϴ�.
		///
		/// ���: �� �޼���� ����� �ڵ忡�� ���� ȣ���ؼ��� �� �˴ϴ�. ���������� �ý��ۿ��� ���˴ϴ�.
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
		/// �� ��� Ž�� �޼��� (���� ������).
		/// �� �޼���� ���� �������� Ȱ��ȭ�� ��� ��� ã�� ��⿭���� ��θ� ����մϴ�.
		///
		/// ����: CalculatePaths
		/// ����: StartPath
		/// </summary>
		void CalculatePathsThreaded (PathHandler pathHandler) {
#if UNITY_2017_3_OR_NEWER
			UnityEngine.Profiling.Profiler.BeginThreadProfiling("Pathfinding", "Pathfinding thread #" + (pathHandler.threadID+1));
#endif

#if !ASTAR_FAST_BUT_NO_EXCEPTIONS
			try {
#endif

				// �� �� �����ϴ� �� ���� �ִ� ƽ ���Դϴ�.
				// �� ƽ�� �и����� 1/10000�Դϴ�.
				// �����尡 �����ؾ� �ϴ��� �ֱ������� Ȯ���ؾ� �մϴ�.
				long maxTicks = (long)(10*10000);
				long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				while (true) 
				{
					// ���� ��� ���� ����Դϴ�
					Path path = queue.Pop();
#if UNITY_2017_3_OR_NEWER
					profilingSampler.Begin();
#endif
					// ���� ���� �޼��忡 �׼���
					IPathInternals ipath = (IPathInternals)path;

					// �� ���� �����带 ������� �ʵ��� ������ ������ �����Ϸ��� �õ��մϴ�.
					if (pathHandler.threadID > 0)
					{
						throw new System.Exception("Thread Error");
					}

					AstarProfiler.StartFastProfile(0);
					ipath.PrepareBase(pathHandler);

					// ���� ��� ó�� ��
					// ó�� �� ���·� ����
					ipath.AdvanceState(PathState.Processing);

					// �Ϻ� �ݹ� ȣ��
					if (OnPathPreSearch != null) 
					{
					OnPathPreSearch(path);
					}

					// ��� ����� ���۵� �ð��� ��Ÿ���� ƽ
					long startTicks = System.DateTime.UtcNow.Ticks;

					// ��� �غ�
					ipath.Prepare();

					AstarProfiler.EndFastProfile(0);

				if (path.CompleteState == PathCompleteState.NotCalculated) 
					{
						// �ð��� ǥ�ø� ���� ���������� ���� ��θ� �����Ͽ� ������(�� ��)���� ����� ������ �� �� �ֵ��� ��
						astar.debugPathData = ipath.PathHandler;
						astar.debugPathID = path.pathID;

						AstarProfiler.StartFastProfile(1);

						// ��� �ʱ�ȭ, ���� �˻��� ������ �غ� ��
						ipath.Initialize();

						AstarProfiler.EndFastProfile(1);

						// ��ΰ� ������ ������ ���� ���� ���� ����
						while (path.CompleteState == PathCompleteState.NotCalculated) {
							// ��� ��꿡 ���� �Ϻ� �۾� ����
							// �Լ��� �ð��� �ʹ� ���� �ɸ��� ��ȯ�ϰų� ����� �Ϸ�Ǹ� ��ȯ
							AstarProfiler.StartFastProfile(2);
							ipath.CalculateStep(targetTick);
							AstarProfiler.EndFastProfile(2);

							targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

							// ť�� ����Ǿ� �� �̻� ��θ� �޾Ƶ����� �ʾƾ� �ϴ� ��� �Լ�(���� �����嵵)�� ����մϴ�.
							// �̰��� A* ��ü�� �ı��Ƿ��� �� �� ����˴ϴ�.
							// ��ΰ� ��ȯ�ǰ� �� �Լ��� ����˴ϴ�.
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

					// ��� �±� �� ��Ÿ ����
					ipath.Cleanup();

					AstarProfiler.StartFastProfile(9);

					if (path.immediateCallback != null) path.immediateCallback(path);

					if (OnPathPostSearch != null) 
					{
						OnPathPostSearch(path);
					}

					// ��θ� ��ȯ ���ÿ� Ǫ���մϴ�
					// �� ���� Unity �����忡�� �����ǰ� �ִ��� ���� ��ȯ�˴ϴ�(���� ���� ������Ʈ���� ���������)
					returnQueue.Enqueue(path);

					// ReturnQueue�� �����
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
				// ó������ ���� ����, ��� ã�� ����
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
		/// �� ��� Ž�� �޼����Դϴ�.
		/// �� �޼���� ��� Ž�� ��⿭���� ��θ� ����մϴ�.
		///
		/// ����: CalculatePathsThreaded
		/// ����: StartPath
		/// </summary>
		IEnumerator CalculatePaths(PathHandler pathHandler)
		{
			// �ڷ�ƾ ������ �ִ� ƽ ��
			long maxTicks = (long)(astar.maxFrameTime*10000);
			long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

			while (true) {
				// ���� ��� ���� ���
				Path p = null;

				AstarProfiler.StartProfile("Path Queue");

				// ���� ����� ��� �������� �õ�
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

				// �� �� ������ �� �ִ� �ִ� ƽ ��
				// �� ƽ�� �и����� 1/10000�Դϴ�.
				maxTicks = (long)(astar.maxFrameTime * 10000);

				ip.PrepareBase(pathHandler);

				// ���� ��� ó�� ��
				// ó�� �� ���·� ��ȯ
				ip.AdvanceState(PathState.Processing);

				// ��� �ݹ� ȣ��
				// ���� ������ ���ϱ� ���� ���� ������ ����Ǿ�� �մϴ�.
				var tmpOnPathPreSearch = OnPathPreSearch;
				if (tmpOnPathPreSearch != null) tmpOnPathPreSearch(p);

				// ��� ����� ���۵� ƽ - ��� �ð� ������ ����
				long startTicks = System.DateTime.UtcNow.Ticks;
				long totalTicks = 0;

				AstarProfiler.StartFastProfile(8);

				AstarProfiler.StartFastProfile(0);
				// ��� �غ�
				AstarProfiler.StartProfile("��� �غ�");
				ip.Prepare();
				AstarProfiler.EndProfile("��� �غ�");
				AstarProfiler.EndFastProfile(0);

				// Prepare ȣ��� ��ΰ� �Ϸ�� ��� Ȯ��
				// �̷��� �Ǹ� �Ϲ������� ��ΰ� �����մϴ�
				if (p.CompleteState == PathCompleteState.NotCalculated)
				{
					// ����׿����� ���������� ���� ��θ� p�� �����Ͽ� ������(�� ��)���� ����� ������ �� �� �ֽ��ϴ�.
					astar.debugPathData = ip.PathHandler;
					astar.debugPathID = p.pathID;

					// ��� �ʱ�ȭ, ���� �˻��� ������ �غ� ��
					AstarProfiler.StartProfile("��� �ʱ�ȭ");
					ip.Initialize();
					AstarProfiler.EndProfile();

					// Init �Լ����� ���� �߻� ���ɼ�
					while (p.CompleteState == PathCompleteState.NotCalculated)
					{
						// ��� ��꿡 ���� �Ϻ� �۾� ����
						// �Լ��� �ð��� ���� �ɸ��ų� ����� �Ϸ�� �� ��ȯ�˴ϴ�
						AstarProfiler.StartFastProfile(2);

						AstarProfiler.StartProfile("��� ��� �ܰ�");
						ip.CalculateStep(targetTick);
						AstarProfiler.EndFastProfile(2);

						AstarProfiler.EndProfile();

						// ��� ����� �Ϸ�Ǹ� ���⿡�� ���� �ߴ��ϴ� ��ſ� ��� ���ϴ�.
						// ��� �ð��� ���̴� �� ������ �˴ϴ�.
						if (p.CompleteState != PathCompleteState.NotCalculated) break;

						AstarProfiler.EndFastProfile(8);
						totalTicks += System.DateTime.UtcNow.Ticks - startTicks;
						// �ٸ� �����尡 �۾��� �� �ֵ��� �纸/���

						AstarProfiler.EndProfile();
						yield return null;
						AstarProfiler.StartProfile("��� ���");

						startTicks = System.DateTime.UtcNow.Ticks;
						AstarProfiler.StartFastProfile(8);

						// ��⿭�� ����Ǿ� �� �̻� ��θ� �޾Ƶ����� �ʾƾ� �ϴ� ��� ��� �Լ�(���� �����嵵) ȣ��
						// �̰��� A* ��ü�� �ı��� �� ����˴ϴ�
						// ��δ� ��ȯ�ǰ� �� �Լ��� ����˴ϴ� (�Լ��� ��ܿ� �ִ� ������ IF �� ����)
						if (queue.IsTerminating)
						{
							p.FailWithError("AstarPath ��ü�� �ı���");
						}

						targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
					}

					totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
					p.duration = totalTicks*0.0001F;

#if ProfileAstar
					System.Threading.Interlocked.Increment(ref AstarPath.PathsCompleted);
#endif
				}

				// ��� �±� ����� �� ��Ÿ ����
				ip.Cleanup();

				AstarProfiler.EndFastProfile(8);

				// ��� �ݹ� ȣ��
				// ���� ������ ���ϱ� ���� ���� ������ ����Ǿ�� �մϴ�.
				var tmpImmediateCallback = p.immediateCallback;
				if (tmpImmediateCallback != null) tmpImmediateCallback(p);

				AstarProfiler.StartFastProfile(13);

				// ���� ������ ���ϱ� ���� ���� ������ ����Ǿ�� �մϴ�.
				var tmpOnPathPostSearch = OnPathPostSearch;
				if (tmpOnPathPostSearch != null) tmpOnPathPostSearch(p);

				AstarProfiler.EndFastProfile(13);

				// ��θ� ��ȯ ���ÿ� Ǫ��
				// ��δ� �� Unity �����忡�� �����ǰ� ������ ���� ��ȯ�� ���Դϴ� (���� ���� ������Ʈ)
				returnQueue.Enqueue(p);

				ip.AdvanceState(PathState.ReturnQueue);

				AstarProfiler.EndProfile();

				// ���� ��θ� ����� ��� ��� ��ٸ��ϴ�
				if (System.DateTime.UtcNow.Ticks > targetTick) {
					yield return null;
					targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				}
			}
		}
	}
}
