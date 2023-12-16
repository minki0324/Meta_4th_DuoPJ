using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	using UnityEngine.Assertions;

#if NETFX_CORE
	using Thread = Pathfinding.WindowsStore.Thread;
#else
	using Thread = System.Threading.Thread;
#endif

	class GraphUpdateProcessor {
		public event System.Action OnGraphsUpdated;

		/// <summary>업데이트할 수 있는 그래프를 보유합니다.</summary>
		readonly AstarPath astar;

#if !UNITY_WEBGL
		/// <summary>
		/// 비동기 그래프 업데이트를 처리하는 스레드에 대한 참조입니다.
		/// 참조: ProcessGraphUpdatesAsync
		/// </summary>
		Thread graphUpdateThread;
#endif

		/// <summary>IsAnyGraphUpdateInProgress에 사용됩니다.</summary>
		bool anyGraphUpdateInProgress;

#if UNITY_2017_3_OR_NEWER && !UNITY_WEBGL
		CustomSampler asyncUpdateProfilingSampler;
#endif

		/// <summary>
		/// 대기 중인 모든 그래프 업데이트 쿼리를 포함하는 큐입니다. AddToQueue를 사용하여 이 큐에 추가할 수 있습니다.
		/// 참조: AddToQueue
		/// </summary>
		readonly Queue<GraphUpdateObject> graphUpdateQueue = new Queue<GraphUpdateObject>();

		/// <summary>실행 대기 중인 모든 비동기 그래프 업데이트의 큐입니다.</summary>
		readonly Queue<GUOSingle> graphUpdateQueueAsync = new Queue<GUOSingle>();

		/// <summary>실행 대기 중인 모든 비동기 그래프 업데이트 후 이벤트의 큐입니다.</summary>
		readonly Queue<GUOSingle> graphUpdateQueuePost = new Queue<GUOSingle>();

		/// <summary>실행 대기 중인 모든 비동기 그래프 업데이트의 큐입니다.</summary>
		readonly Queue<GUOSingle> graphUpdateQueueRegular = new Queue<GUOSingle>();

		readonly System.Threading.ManualResetEvent asyncGraphUpdatesComplete = new System.Threading.ManualResetEvent(true);

#if !UNITY_WEBGL
		readonly System.Threading.AutoResetEvent graphUpdateAsyncEvent = new System.Threading.AutoResetEvent(false);
		readonly System.Threading.AutoResetEvent exitAsyncThread = new System.Threading.AutoResetEvent(false);
#endif

		/// <summary>대기 중인 그래프 업데이트가 있는지 여부를 반환합니다.</summary>
		public bool IsAnyGraphUpdateQueued { get { return graphUpdateQueue.Count > 0; } }

		/// <summary>진행 중인 그래프 업데이트가 있는지 여부를 반환합니다.</summary>
		public bool IsAnyGraphUpdateInProgress { get { return anyGraphUpdateInProgress; } }

		/// <summary>그래프 업데이트 순서를 나타내는 열거형입니다.</summary>
		enum GraphUpdateOrder
		{
			GraphUpdate,
			// FloodFill
		}

		/// <summary>그래프에 대한 수행해야 할 단일 업데이트를 보유합니다.</summary>
		struct GUOSingle
		{
			public GraphUpdateOrder order;
			public IUpdatableGraph graph;
			public GraphUpdateObject obj;
		}

		public GraphUpdateProcessor (AstarPath astar) {
			this.astar = astar;
		}

		/// <summary>모든 대기열 업데이트를 적용하는 데 사용할 수 있는 작업 항목을 가져옵니다.</summary>
		public AstarWorkItem GetWorkItem()
		{
			return new AstarWorkItem(QueueGraphUpdatesInternal, ProcessGraphUpdates);
		}

		public void EnableMultithreading () {
#if !UNITY_WEBGL
			if (graphUpdateThread == null || !graphUpdateThread.IsAlive) {
#if UNITY_2017_3_OR_NEWER && !UNITY_WEBGL
				asyncUpdateProfilingSampler = CustomSampler.Create("Graph Update");
#endif

				graphUpdateThread = new Thread(ProcessGraphUpdatesAsync);
				graphUpdateThread.IsBackground = true;

				// 그래프 업데이트에 대한 스레드 우선 순위 설정
				// Windows 스토어 또는 Windows Phone을 컴파일하지 않는 한
#if !UNITY_WINRT
				graphUpdateThread.Priority = System.Threading.ThreadPriority.Lowest;
#endif
				graphUpdateThread.Start();
			}
#endif
		}

		public void DisableMultithreading () {
#if !UNITY_WEBGL
			if (graphUpdateThread != null && graphUpdateThread.IsAlive) {
				// 그래프 업데이트 스레드를 재개하여 스레드를 종료하도록 합니다
				exitAsyncThread.Set();

				if (!graphUpdateThread.Join(5*1000)) {
					Debug.LogError("그래프 업데이트 스레드가 5 초 안에 종료되지 않았습니다");
				}

				graphUpdateThread = null;
			}
#endif
		}

		/// <summary>
		/// GraphUpdateObject를 사용하여 모든 그래프를 업데이트합니다.
		/// 예를 들어, 특정 영역의 모든 노드를 걷기 불가능하게 만들거나 더 높은 패널티로 설정하는 데 사용할 수 있습니다.
		/// 그래프는 가능한 빨리 업데이트됩니다 (AstarPath.batchGraphUpdates에 따라).
		///
		/// 참조: FlushGraphUpdates
		/// </summary>
		public void AddToQueue (GraphUpdateObject ob) {
			// GUO를 큐에 추가합니다.
			graphUpdateQueue.Enqueue(ob);
		}

		/// <summary>그래프 업데이트를 내부적으로 예약합니다.</summary>
		void QueueGraphUpdatesInternal () {
			while (graphUpdateQueue.Count > 0) {
				GraphUpdateObject ob = graphUpdateQueue.Dequeue();
				if (ob.internalStage != GraphUpdateObject.STAGE_PENDING) {
					Debug.LogError("남은 그래프 업데이트가 대기 중인 것으로 예상됩니다.");
					continue;
				}
				ob.internalStage = 0;

				foreach (IUpdatableGraph g in astar.data.GetUpdateableGraphs()) {
					NavGraph gr = g as NavGraph;
					if (ob.nnConstraint == null || ob.nnConstraint.SuitableGraph(astar.data.GetGraphIndex(gr), gr)) {
						var guo = new GUOSingle();
						guo.order = GraphUpdateOrder.GraphUpdate;
						guo.obj = ob;
						guo.graph = g;
						ob.internalStage += 1;
						graphUpdateQueueRegular.Enqueue(guo);
					}
				}
			}

			GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);
			anyGraphUpdateInProgress = true;
		}

		/// <summary>
		/// 그래프 업데이트를 처리합니다.
		/// 몇몇 그래프 업데이트를 수행하고, 다른 스레드에게 수행하도록 신호를 보낼 수 있습니다.
		/// QueueGraphUpdatesInternal에서 추가한 그래프 업데이트만 처리합니다.
		///
		/// 반환값: True인 경우 모든 그래프 업데이트가 완료되었으며 경로 찾기 (또는 다른 작업)를 다시 시작할 수 있습니다.
		/// False인 경우 아직 처리 중이거나 큐에서 대기 중인 그래프 업데이트가 있습니다.
		/// </summary>
		/// <param name="force">True인 경우 이 함수가 반환하기 전에 모든 그래프 업데이트가 처리됩니다. 반환값은 True입니다.</param>
		bool ProcessGraphUpdates(bool force)
		{
			Assert.IsTrue(anyGraphUpdateInProgress);

			if (force) {
				asyncGraphUpdatesComplete.WaitOne();
			} else {
#if !UNITY_WEBGL
				if (!asyncGraphUpdatesComplete.WaitOne(0)) {
					return false;
				}
#endif
			}

			Assert.AreEqual(graphUpdateQueueAsync.Count, 0, "이 시점에서 큐는 비어 있어야 합니다.");

			ProcessPostUpdates();
			if (!ProcessRegularUpdates(force)) {
				return false;
			}

			GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
			if (OnGraphsUpdated != null) OnGraphsUpdated();

			Assert.AreEqual(graphUpdateQueueAsync.Count, 0, "이 시점에서 큐는 비어 있어야 합니다.");

			ProcessPostUpdates();
			return true;
		}

		bool ProcessRegularUpdates (bool force) {
			while (graphUpdateQueueRegular.Count > 0) {
				GUOSingle s = graphUpdateQueueRegular.Peek();

				// 그래프 업데이트의 스레딩 방식을 가져옵니다.
				GraphUpdateThreading threading = s.graph.CanUpdateAsync(s.obj);

#if UNITY_WEBGL
				// WebGL에서는 다중 스레딩을 사용하지 않습니다.
#else
				// 재생 중이 아니거나 그래프 업데이트 스레드를 사용하지 않는 경우 또는 해당 스레드가 종료된 경우에만 Unity 스레드에서 모두 실행합니다.
				if (force || !Application.isPlaying || graphUpdateThread == null || !graphUpdateThread.IsAlive)
				{
					// SeparateThread 플래그 제거
					threading &= ~GraphUpdateThreading.SeparateThread;
				}
#endif

				if ((threading & GraphUpdateThreading.UnityInit) != 0) {
					// 비동기 그래프 업데이트를 먼저 처리합니다.
					// 다음 이 함수 호출은 이 개체를 처리하기 때문에 지금은 큐에서 제거되지 않습니다.
					if (StartAsyncUpdatesIfQueued())
					{
						return false;
					}

					s.graph.UpdateAreaInit(s.obj);
				}

				if ((threading & GraphUpdateThreading.SeparateThread) != 0) {
					// GUO를 비동기 큐로 이동하여 다른 스레드에서 업데이트하도록 합니다.
					graphUpdateQueueRegular.Dequeue();
					graphUpdateQueueAsync.Enqueue(s);

					// 이 업데이트 후에 Unity 스레드 함수에서 실행되어야 하지만
					// 다음 업데이트가 시작되기 전에 실행되어야 하므로 더 이상 비동기 그래프 업데이트를 시작하지 않습니다.
					if ((threading & GraphUpdateThreading.UnityPost) != 0)
					{
						if (StartAsyncUpdatesIfQueued()) {
							return false;
						}
					}
				} else {
					// Unity 스레드

					if (StartAsyncUpdatesIfQueued()) {
						return false;
					}

					graphUpdateQueueRegular.Dequeue();

					try {
						s.graph.UpdateArea(s.obj);
					} catch (System.Exception e) {
						Debug.LogError("그래프 업데이트 중 오류 발생\n" + e);
					}

					if ((threading & GraphUpdateThreading.UnityPost) != 0) {
						s.graph.UpdateAreaPost(s.obj);
					}

					s.obj.internalStage -= 1;
					UnityEngine.Assertions.Assert.IsTrue(s.obj.internalStage >= 0);
				}
			}

			if (StartAsyncUpdatesIfQueued()) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// 다른 스레드가 <see cref="graphUpdateQueueAsync"/> 큐에 있는 그래프 업데이트를 처리하도록 그래프 업데이트 스레드에 신호를 보냅니다.
		/// 반환값: 다른 스레드에 신호가 보내진 경우 True입니다.
		/// </summary>
		bool StartAsyncUpdatesIfQueued()
		{
			if (graphUpdateQueueAsync.Count > 0) {
#if UNITY_WEBGL
				throw new System.Exception("WebGL에서는 이러한 상황이 발생하지 않아야 합니다.");
#else
				asyncGraphUpdatesComplete.Reset();
				graphUpdateAsyncEvent.Set();
				return true;
#endif
			}
			return false;
		}

		void ProcessPostUpdates () {
			while (graphUpdateQueuePost.Count > 0) {
				GUOSingle s = graphUpdateQueuePost.Dequeue();

				GraphUpdateThreading threading = s.graph.CanUpdateAsync(s.obj);

				if ((threading & GraphUpdateThreading.UnityPost) != 0) {
					try {
						s.graph.UpdateAreaPost(s.obj);
					} catch (System.Exception e) {
						Debug.LogError("그래프 업데이트 중 오류 발생 (후처리 단계)\n" + e);
					}
				}

				s.obj.internalStage -= 1;
				UnityEngine.Assertions.Assert.IsTrue(s.obj.internalStage >= 0);
			}
		}

#if !UNITY_WEBGL
		/// <summary>
		/// 그래프 업데이트 스레드입니다.
		/// 비동기 그래프 업데이트는 이 메서드에서 다른 스레드에서 실행됩니다.
		/// </summary>
		void ProcessGraphUpdatesAsync () {
#if UNITY_2017_3_OR_NEWER
			Profiler.BeginThreadProfiling("Pathfinding", "Threaded Graph Updates");
#endif

			var handles = new [] { graphUpdateAsyncEvent, exitAsyncThread };

			while (true) {
				// 다음 일괄 처리 또는 종료 이벤트를 대기합니다.
				var handleIndex = WaitHandle.WaitAny(handles);

				if (handleIndex == 1)
				{
					// 종료 이벤트가 발생했습니다.
					// 스레드를 중단하고 큐를 지웁니다.
					while (graphUpdateQueueAsync.Count > 0) {
						var s = graphUpdateQueueAsync.Dequeue();
						s.obj.internalStage = GraphUpdateObject.STAGE_ABORTED;
					}
					asyncGraphUpdatesComplete.Set();
#if UNITY_2017_3_OR_NEWER
					Profiler.EndThreadProfiling();
#endif
					return;
				}

				while (graphUpdateQueueAsync.Count > 0) {
#if UNITY_2017_3_OR_NEWER
					asyncUpdateProfilingSampler.Begin();
#endif
					// 주의: 비동기 그래프 업데이트가 큐에 락 없이 추가되므로
					// 주 스레드에서 액세스할 수 없습니다.
					GUOSingle aguo = graphUpdateQueueAsync.Dequeue();

					try {
						if (aguo.order == GraphUpdateOrder.GraphUpdate) {
							aguo.graph.UpdateArea(aguo.obj);
							graphUpdateQueuePost.Enqueue(aguo);
						} else {
							throw new System.NotSupportedException("" + aguo.order);
						}
					} catch (System.Exception e) {
						Debug.LogError("그래프 업데이트 중 예외 발생:\n" + e);
					}
#if UNITY_2017_3_OR_NEWER
					asyncUpdateProfilingSampler.End();
#endif
				}

				// Done
				asyncGraphUpdatesComplete.Set();
			}
		}
#endif
	}
}
