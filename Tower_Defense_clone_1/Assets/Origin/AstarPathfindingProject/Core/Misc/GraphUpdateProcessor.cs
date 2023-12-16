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

		/// <summary>������Ʈ�� �� �ִ� �׷����� �����մϴ�.</summary>
		readonly AstarPath astar;

#if !UNITY_WEBGL
		/// <summary>
		/// �񵿱� �׷��� ������Ʈ�� ó���ϴ� �����忡 ���� �����Դϴ�.
		/// ����: ProcessGraphUpdatesAsync
		/// </summary>
		Thread graphUpdateThread;
#endif

		/// <summary>IsAnyGraphUpdateInProgress�� ���˴ϴ�.</summary>
		bool anyGraphUpdateInProgress;

#if UNITY_2017_3_OR_NEWER && !UNITY_WEBGL
		CustomSampler asyncUpdateProfilingSampler;
#endif

		/// <summary>
		/// ��� ���� ��� �׷��� ������Ʈ ������ �����ϴ� ť�Դϴ�. AddToQueue�� ����Ͽ� �� ť�� �߰��� �� �ֽ��ϴ�.
		/// ����: AddToQueue
		/// </summary>
		readonly Queue<GraphUpdateObject> graphUpdateQueue = new Queue<GraphUpdateObject>();

		/// <summary>���� ��� ���� ��� �񵿱� �׷��� ������Ʈ�� ť�Դϴ�.</summary>
		readonly Queue<GUOSingle> graphUpdateQueueAsync = new Queue<GUOSingle>();

		/// <summary>���� ��� ���� ��� �񵿱� �׷��� ������Ʈ �� �̺�Ʈ�� ť�Դϴ�.</summary>
		readonly Queue<GUOSingle> graphUpdateQueuePost = new Queue<GUOSingle>();

		/// <summary>���� ��� ���� ��� �񵿱� �׷��� ������Ʈ�� ť�Դϴ�.</summary>
		readonly Queue<GUOSingle> graphUpdateQueueRegular = new Queue<GUOSingle>();

		readonly System.Threading.ManualResetEvent asyncGraphUpdatesComplete = new System.Threading.ManualResetEvent(true);

#if !UNITY_WEBGL
		readonly System.Threading.AutoResetEvent graphUpdateAsyncEvent = new System.Threading.AutoResetEvent(false);
		readonly System.Threading.AutoResetEvent exitAsyncThread = new System.Threading.AutoResetEvent(false);
#endif

		/// <summary>��� ���� �׷��� ������Ʈ�� �ִ��� ���θ� ��ȯ�մϴ�.</summary>
		public bool IsAnyGraphUpdateQueued { get { return graphUpdateQueue.Count > 0; } }

		/// <summary>���� ���� �׷��� ������Ʈ�� �ִ��� ���θ� ��ȯ�մϴ�.</summary>
		public bool IsAnyGraphUpdateInProgress { get { return anyGraphUpdateInProgress; } }

		/// <summary>�׷��� ������Ʈ ������ ��Ÿ���� �������Դϴ�.</summary>
		enum GraphUpdateOrder
		{
			GraphUpdate,
			// FloodFill
		}

		/// <summary>�׷����� ���� �����ؾ� �� ���� ������Ʈ�� �����մϴ�.</summary>
		struct GUOSingle
		{
			public GraphUpdateOrder order;
			public IUpdatableGraph graph;
			public GraphUpdateObject obj;
		}

		public GraphUpdateProcessor (AstarPath astar) {
			this.astar = astar;
		}

		/// <summary>��� ��⿭ ������Ʈ�� �����ϴ� �� ����� �� �ִ� �۾� �׸��� �����ɴϴ�.</summary>
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

				// �׷��� ������Ʈ�� ���� ������ �켱 ���� ����
				// Windows ����� �Ǵ� Windows Phone�� ���������� �ʴ� ��
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
				// �׷��� ������Ʈ �����带 �簳�Ͽ� �����带 �����ϵ��� �մϴ�
				exitAsyncThread.Set();

				if (!graphUpdateThread.Join(5*1000)) {
					Debug.LogError("�׷��� ������Ʈ �����尡 5 �� �ȿ� ������� �ʾҽ��ϴ�");
				}

				graphUpdateThread = null;
			}
#endif
		}

		/// <summary>
		/// GraphUpdateObject�� ����Ͽ� ��� �׷����� ������Ʈ�մϴ�.
		/// ���� ���, Ư�� ������ ��� ��带 �ȱ� �Ұ����ϰ� ����ų� �� ���� �г�Ƽ�� �����ϴ� �� ����� �� �ֽ��ϴ�.
		/// �׷����� ������ ���� ������Ʈ�˴ϴ� (AstarPath.batchGraphUpdates�� ����).
		///
		/// ����: FlushGraphUpdates
		/// </summary>
		public void AddToQueue (GraphUpdateObject ob) {
			// GUO�� ť�� �߰��մϴ�.
			graphUpdateQueue.Enqueue(ob);
		}

		/// <summary>�׷��� ������Ʈ�� ���������� �����մϴ�.</summary>
		void QueueGraphUpdatesInternal () {
			while (graphUpdateQueue.Count > 0) {
				GraphUpdateObject ob = graphUpdateQueue.Dequeue();
				if (ob.internalStage != GraphUpdateObject.STAGE_PENDING) {
					Debug.LogError("���� �׷��� ������Ʈ�� ��� ���� ������ ����˴ϴ�.");
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
		/// �׷��� ������Ʈ�� ó���մϴ�.
		/// ��� �׷��� ������Ʈ�� �����ϰ�, �ٸ� �����忡�� �����ϵ��� ��ȣ�� ���� �� �ֽ��ϴ�.
		/// QueueGraphUpdatesInternal���� �߰��� �׷��� ������Ʈ�� ó���մϴ�.
		///
		/// ��ȯ��: True�� ��� ��� �׷��� ������Ʈ�� �Ϸ�Ǿ����� ��� ã�� (�Ǵ� �ٸ� �۾�)�� �ٽ� ������ �� �ֽ��ϴ�.
		/// False�� ��� ���� ó�� ���̰ų� ť���� ��� ���� �׷��� ������Ʈ�� �ֽ��ϴ�.
		/// </summary>
		/// <param name="force">True�� ��� �� �Լ��� ��ȯ�ϱ� ���� ��� �׷��� ������Ʈ�� ó���˴ϴ�. ��ȯ���� True�Դϴ�.</param>
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

			Assert.AreEqual(graphUpdateQueueAsync.Count, 0, "�� �������� ť�� ��� �־�� �մϴ�.");

			ProcessPostUpdates();
			if (!ProcessRegularUpdates(force)) {
				return false;
			}

			GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
			if (OnGraphsUpdated != null) OnGraphsUpdated();

			Assert.AreEqual(graphUpdateQueueAsync.Count, 0, "�� �������� ť�� ��� �־�� �մϴ�.");

			ProcessPostUpdates();
			return true;
		}

		bool ProcessRegularUpdates (bool force) {
			while (graphUpdateQueueRegular.Count > 0) {
				GUOSingle s = graphUpdateQueueRegular.Peek();

				// �׷��� ������Ʈ�� ������ ����� �����ɴϴ�.
				GraphUpdateThreading threading = s.graph.CanUpdateAsync(s.obj);

#if UNITY_WEBGL
				// WebGL������ ���� �������� ������� �ʽ��ϴ�.
#else
				// ��� ���� �ƴϰų� �׷��� ������Ʈ �����带 ������� �ʴ� ��� �Ǵ� �ش� �����尡 ����� ��쿡�� Unity �����忡�� ��� �����մϴ�.
				if (force || !Application.isPlaying || graphUpdateThread == null || !graphUpdateThread.IsAlive)
				{
					// SeparateThread �÷��� ����
					threading &= ~GraphUpdateThreading.SeparateThread;
				}
#endif

				if ((threading & GraphUpdateThreading.UnityInit) != 0) {
					// �񵿱� �׷��� ������Ʈ�� ���� ó���մϴ�.
					// ���� �� �Լ� ȣ���� �� ��ü�� ó���ϱ� ������ ������ ť���� ���ŵ��� �ʽ��ϴ�.
					if (StartAsyncUpdatesIfQueued())
					{
						return false;
					}

					s.graph.UpdateAreaInit(s.obj);
				}

				if ((threading & GraphUpdateThreading.SeparateThread) != 0) {
					// GUO�� �񵿱� ť�� �̵��Ͽ� �ٸ� �����忡�� ������Ʈ�ϵ��� �մϴ�.
					graphUpdateQueueRegular.Dequeue();
					graphUpdateQueueAsync.Enqueue(s);

					// �� ������Ʈ �Ŀ� Unity ������ �Լ����� ����Ǿ�� ������
					// ���� ������Ʈ�� ���۵Ǳ� ���� ����Ǿ�� �ϹǷ� �� �̻� �񵿱� �׷��� ������Ʈ�� �������� �ʽ��ϴ�.
					if ((threading & GraphUpdateThreading.UnityPost) != 0)
					{
						if (StartAsyncUpdatesIfQueued()) {
							return false;
						}
					}
				} else {
					// Unity ������

					if (StartAsyncUpdatesIfQueued()) {
						return false;
					}

					graphUpdateQueueRegular.Dequeue();

					try {
						s.graph.UpdateArea(s.obj);
					} catch (System.Exception e) {
						Debug.LogError("�׷��� ������Ʈ �� ���� �߻�\n" + e);
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
		/// �ٸ� �����尡 <see cref="graphUpdateQueueAsync"/> ť�� �ִ� �׷��� ������Ʈ�� ó���ϵ��� �׷��� ������Ʈ �����忡 ��ȣ�� �����ϴ�.
		/// ��ȯ��: �ٸ� �����忡 ��ȣ�� ������ ��� True�Դϴ�.
		/// </summary>
		bool StartAsyncUpdatesIfQueued()
		{
			if (graphUpdateQueueAsync.Count > 0) {
#if UNITY_WEBGL
				throw new System.Exception("WebGL������ �̷��� ��Ȳ�� �߻����� �ʾƾ� �մϴ�.");
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
						Debug.LogError("�׷��� ������Ʈ �� ���� �߻� (��ó�� �ܰ�)\n" + e);
					}
				}

				s.obj.internalStage -= 1;
				UnityEngine.Assertions.Assert.IsTrue(s.obj.internalStage >= 0);
			}
		}

#if !UNITY_WEBGL
		/// <summary>
		/// �׷��� ������Ʈ �������Դϴ�.
		/// �񵿱� �׷��� ������Ʈ�� �� �޼��忡�� �ٸ� �����忡�� ����˴ϴ�.
		/// </summary>
		void ProcessGraphUpdatesAsync () {
#if UNITY_2017_3_OR_NEWER
			Profiler.BeginThreadProfiling("Pathfinding", "Threaded Graph Updates");
#endif

			var handles = new [] { graphUpdateAsyncEvent, exitAsyncThread };

			while (true) {
				// ���� �ϰ� ó�� �Ǵ� ���� �̺�Ʈ�� ����մϴ�.
				var handleIndex = WaitHandle.WaitAny(handles);

				if (handleIndex == 1)
				{
					// ���� �̺�Ʈ�� �߻��߽��ϴ�.
					// �����带 �ߴ��ϰ� ť�� ����ϴ�.
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
					// ����: �񵿱� �׷��� ������Ʈ�� ť�� �� ���� �߰��ǹǷ�
					// �� �����忡�� �׼����� �� �����ϴ�.
					GUOSingle aguo = graphUpdateQueueAsync.Dequeue();

					try {
						if (aguo.order == GraphUpdateOrder.GraphUpdate) {
							aguo.graph.UpdateArea(aguo.obj);
							graphUpdateQueuePost.Enqueue(aguo);
						} else {
							throw new System.NotSupportedException("" + aguo.order);
						}
					} catch (System.Exception e) {
						Debug.LogError("�׷��� ������Ʈ �� ���� �߻�:\n" + e);
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
