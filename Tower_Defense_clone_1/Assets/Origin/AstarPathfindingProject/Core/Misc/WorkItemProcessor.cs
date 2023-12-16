using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	using UnityEngine;

	/// <summary>
	/// �׷����� ������Ʈ�� �� ������ �� �ִ� �۾� �׸��Դϴ�.
	/// ����: <see cref="AstarPath.UpdateGraphs"/>
	/// ����: <see cref="AstarPath.AddWorkItem"/>
	/// </summary>
	public struct AstarWorkItem {
		/// <summary>
		/// �ʱ�ȭ �Լ�.
		/// �ʱ�ȭ�� �ʿ����� ���� ��� null�� �� �ֽ��ϴ�.
		/// <see cref="update"/> ȣ�� ������ �� �� ȣ��˴ϴ�.
		/// </summary>
		public System.Action init;

		/// <summary>
		/// �ʱ�ȭ �Լ�.
		/// �ʱ�ȭ�� �ʿ����� ���� ��� null�� �� �ֽ��ϴ�.
		/// <see cref="update"/> ȣ�� ������ �� �� ȣ��˴ϴ�.
		///
		/// ���ؽ�Ʈ ��ü�� �Ű������� ���޵˴ϴ�. �̸� ����Ͽ� ���� ��� ȫ�� ä���(flood fill)�� ��⿭�� ���� �� ������,
		/// ȫ�� ä���� �۾� �׸��� <see cref="EnsureValidFloodFill"/>�� ȣ���ϰų� ��� �۾� �׸��� �Ϸ�� �� ����˴ϴ�.
		/// ��带 ������Ʈ�ϴ� ���� �۾� �׸��� ȫ�� ä��⸦ �ʿ�� �ϴ� ��� <see cref="QueueFloodFill"/> �޼��带 ����ϴ� ���� �����ϴ�.
		/// �۾� �׸񸶴� ȫ�� ä��⸦ �����ϴ� ��� ��� �۾� �׸� ���� �ϳ��� ȫ�� ä��⸸ ����Ǳ� �����Դϴ�.
		/// </summary>
		public System.Action<IWorkItemContext> initWithContext;

		/// <summary>
		/// �۾� �׸��� ����� �� �� �� �����Ӵ� ȣ��Ǵ� ������Ʈ �Լ��Դϴ�.
		/// force �Ű������� ���մϴ�. true�� ��� �۾� �׸��� ���� �����ӿ� ���� ������ �ʰ� �� ���� �Ϸ��Ϸ��� �õ��ؾ� �մϴ�.
		/// ��ȯ��: �۾� �׸��� �Ϸ�Ǹ� true.
		/// </summary>
		public System.Func<bool, bool> update;

		/// <summary>
		/// �۾� �׸��� ����� �� �� �� �����Ӵ� ȣ��Ǵ� ������Ʈ �Լ��Դϴ�.
		/// force �Ű������� ���մϴ�. true�� ��� �۾� �׸��� ���� �����ӿ� ���� ������ �ʰ� �� ���� �Ϸ��Ϸ��� �õ��ؾ� �մϴ�.
		/// ��ȯ��: �۾� �׸��� �Ϸ�Ǹ� true.
		///
		/// ���ؽ�Ʈ ��ü�� �Ű������� ���޵˴ϴ�. �̸� ����Ͽ� ���� ��� ȫ�� ä���(flood fill)�� ��⿭�� ���� �� ������,
		/// ȫ�� ä���� �۾� �׸��� <see cref="EnsureValidFloodFill"/>�� ȣ���ϰų� ��� �۾� �׸��� �Ϸ�� �� ����˴ϴ�.
		/// ��带 ������Ʈ�ϴ� ���� �۾� �׸��� ȫ�� ä��⸦ �ʿ�� �ϴ� ��� <see cref="QueueFloodFill"/> �޼��带 ����ϴ� ���� �����ϴ�.
		/// �۾� �׸񸶴� ȫ�� ä��⸦ �����ϴ� ��� ��� �۾� �׸� ���� �ϳ��� ȫ�� ä��⸸ ����Ǳ� �����Դϴ�.
		/// </summary>
		public System.Func<IWorkItemContext, bool, bool> updateWithContext;

		public AstarWorkItem (System.Func<bool, bool> update) {
			this.init = null;
			this.initWithContext = null;
			this.updateWithContext = null;
			this.update = update;
		}

		public AstarWorkItem (System.Func<IWorkItemContext, bool, bool> update) {
			this.init = null;
			this.initWithContext = null;
			this.updateWithContext = update;
			this.update = null;
		}

		public AstarWorkItem (System.Action init, System.Func<bool, bool> update = null) {
			this.init = init;
			this.initWithContext = null;
			this.update = update;
			this.updateWithContext = null;
		}

		public AstarWorkItem (System.Action<IWorkItemContext> init, System.Func<IWorkItemContext, bool, bool> update = null) {
			this.init = null;
			this.initWithContext = init;
			this.update = null;
			this.updateWithContext = update;
		}
	}

	/// <summary>WorkItemProcessor ����� �Ϻθ� �����ϴ� �������̽�</summary>
	public interface IWorkItemContext
	{
		/// <summary>
		/// �۾� �׸� ������ ȣ���Ͽ� ȫ�� ä���(flood fill)�� ��⿭�� �߰��մϴ�.
		/// ȫ�� ä��⸦ ��� �����Ϸ��� FloodFill()�� ����� �� ������,
		/// �� �޼��带 ����Ͽ� ���� ������Ʈ�� �ϳ��� ���� ������ ����ų �� �ֽ��ϴ�.
		/// �۾� �׸��� ���� �߿� ��ȿ�� ȫ�� ä��Ⱑ �ʿ��� ��� <see cref="EnsureValidFloodFill"/>�� ȣ���Ͽ�
		/// ��⿭�� ȫ�� ä��Ⱑ �߰��Ǿ����� Ȯ���� �� �ֽ��ϴ�.
		///
		/// ȫ�� ä��Ⱑ ��⿭�� �߰��Ǹ� ��� �۾� �׸��� ����� �Ŀ� ����˴ϴ�.
		///
		/// ��� �Ұ�: ������� �ʴ� ���� �����ϴ�. �� �޼��带 ����ϸ� ���� ���� ����� ������ ������ �����˴ϴ�. ��κ��� ��� ���� �׷��� Ŭ������ �޴ܿ��� �۾��� �ڵ����� ó���ϹǷ� �� �Լ� ȣ���� ������ �� �־�� �մϴ�.
		/// </summary>
		[System.Obsolete("����� ���ϼ���. �� �޼��带 ����ϸ� ����� ���� ����� ������ ������ �����˴ϴ�. ��κ��� ��� ���� �׷��� Ŭ������ �޴ܿ��� �۾��� �ڵ����� ó���ϹǷ� �� �Լ� ȣ���� ������ �� �־�� �մϴ�.")]
		void QueueFloodFill();

		/// <summary>
		/// �۾� �׸��� ���� �߿� ��ȿ�� ���� ������ �ʿ��� ��� ����Ͽ� ��⿭�� ȫ�� ä���(flood fill)�� ������ Ȯ���մϴ�.
		/// �۾� �׸񿡼� <see cref="Pathfinding.GraphNode.Area"/> �Ӽ� �Ǵ� <see cref="Pathfinding.PathUtilities.IsPathPossible"/> �޼��带 ����ϴ� ���
		/// ����ϱ� ���� �� �޼��带 ȣ���Ͽ� �����Ͱ� �ֽ� �������� Ȯ���� �� �ֽ��ϴ�.
		///
		/// ����: <see cref="Pathfinding.HierarchicalGraph"/>
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(new AstarWorkItem((IWorkItemContext ctx) => {
		///     ctx.EnsureValidFloodFill();
		///
		///     // ���� ȣ���� �� �޼��尡 �׷����� ���� �ֽ� ������ ������ �ִ� ���� �����մϴ�
		///     if (PathUtilities.IsPathPossible(someNode, someOtherNode)) {
		///         // ���𰡸� �����մϴ�
		///     }
		/// }));
		/// </code>
		/// </summary>
		void EnsureValidFloodFill();

		/// <summary>
		/// �׷��� ���� �̺�Ʈ�� Ʈ�����մϴ�.
		/// �̷� ���� <see cref="Pathfinding.GraphModifier.PostUpdate"/> �̺�Ʈ�� ��� �׷��� ������Ʈ�� �Ϸ�� �Ŀ� �߻��մϴ�.
		/// �Ϻ� ��ũ��Ʈ�� �� �̺�Ʈ�� û���մϴ�. ���� ��� ����-�޽� ��ũ�� û���ϰ� ������ �� �̺�Ʈ�� ���۵� �� ����� ��带 �ٽ� ����մϴ�.
		/// �׷����� ���� �� �����ǰų� ���� ���� �׷����� ������ ��� �̺�Ʈ�� �� ���� ���۵˴ϴ�.
		/// </summary>
		void SetGraphDirty(NavGraph graph);
	}

	class WorkItemProcessor : IWorkItemContext {
		/// <summary>�۾� �׸��� �ٸ� �۾� �׸� ������ �ϷḦ ����ϴ� ���� �����ϱ� ���� ���˴ϴ�. �̷� ���� ���α׷��� ���� �� �ֽ��ϴ�.</summary>
		public bool workItemsInProgressRightNow { get; private set; }

		readonly AstarPath astar;
		readonly IndexedQueue<AstarWorkItem> workItems = new IndexedQueue<AstarWorkItem>();

		/// <summary>���� ��� ���� �۾� �׸��� �ִ� ��� true</summary>
		public bool anyQueued {
			get { return workItems.Count > 0; }
		}

		/// <summary>
		/// ���� �۾� �׸��� ȫ�� ä��⸦ ��� ���� ��� true�Դϴ�.
		/// ����: QueueWorkItemFloodFill
		/// </summary>
		bool queuedWorkItemFloodFill = false;

		bool anyGraphsDirty = true;

		/// <summary>
		/// �۾� �׸� �ϰ� ó�� ���� �� true�Դϴ�.
		/// �۾� �׸� ������Ʈ�� ���� ���� �����ӿ� ���� �л�ǹǷ��� �÷��״� ������Ʈ�� ���� ���� ���� �׻� true�Դϴ�.
		/// </summary>
		public bool workItemsInProgress { get; private set; }

		/// <summary>Queue<T>�� ���������� ������ �׼����� ����մϴ�</summary>
		class IndexedQueue<T> {
			T[] buffer = new T[4];
			int start;

			public T this[int index] {
				get {
					if (index < 0 || index >= Count) throw new System.IndexOutOfRangeException();
					return buffer[(start + index) % buffer.Length];
				}
				set {
					if (index < 0 || index >= Count) throw new System.IndexOutOfRangeException();
					buffer[(start + index) % buffer.Length] = value;
				}
			}

			public int Count { get; private set; }

			public void Enqueue (T item) {
				if (Count == buffer.Length) {
					var newBuffer = new T[buffer.Length*2];
					for (int i = 0; i < Count; i++) {
						newBuffer[i] = this[i];
					}
					buffer = newBuffer;
					start = 0;
				}

				buffer[(start + Count) % buffer.Length] = item;
				Count++;
			}

			public T Dequeue () {
				if (Count == 0) throw new System.InvalidOperationException();
				var item = buffer[start];
				start = (start + 1) % buffer.Length;
				Count--;
				return item;
			}
		}

		/// <summary>
		/// �۾� �׸� ���� �߿� ȫ�� ä��⸦ ��⿭�� �߰��Ϸ��� ȣ���մϴ�.
		/// ��� ȫ�� ä���� FloodFill()�� ���� ������ �� ������ ���� ������Ʈ�� �ϳ��� ���� ������ ����Ű�� ���� �� �޼��带 ����� �� �ֽ��ϴ�.
		/// ���� ���� �۾� �׸��� ������ ȫ�� ä��⸦ ��⿭�� �߰��� ���, ��� �۾� �׸��� ����� �Ŀ� ȫ�� ä��Ⱑ ����˴ϴ�.
		/// </summary>
		void IWorkItemContext.QueueFloodFill () {
			queuedWorkItemFloodFill = true;
		}

		void IWorkItemContext.SetGraphDirty (NavGraph graph) {
			anyGraphsDirty = true;
		}

		/// <summary>�۾� �׸��� ���� �߿� ��ȿ�� ���� ������ �ʿ��� ��� ȣ���Ͽ� ��� ���� ȫ�� ä��Ⱑ ������ Ȯ���մϴ�.</summary>
		public void EnsureValidFloodFill()
		{
			if (queuedWorkItemFloodFill) {
				astar.hierarchicalGraph.RecalculateAll();
			} else {
				astar.hierarchicalGraph.RecalculateIfNecessary();
			}
		}

		public WorkItemProcessor (AstarPath astar) {
			this.astar = astar;
		}

		public void OnFloodFill () {
			queuedWorkItemFloodFill = false;
		}

		/// <summary>
		/// �۾� �׸��� �߰��Ͽ� ��� ã�Ⱑ �Ͻ� ������ �� ó���ǵ����մϴ�.
		/// ����: ProcessWorkItems
		/// </summary>
		public void AddWorkItem(AstarWorkItem item)
		{
			workItems.Enqueue(item);
		}

		/// <summary>
		/// �׷��� ������Ʈ �۾� �׸��� ó���մϴ�.
		/// ��� ��� ���� �۾� �׸��� ó���մϴ�. ���� ��� �׷��� ������Ʈ ���� ó���մϴ�.
		///
		/// ��ȯ��:
		/// - false: ���� ó���ؾ� �� �׸��� ���� ���
		/// - true: ������ �۾� �׸��� ó���ǰ� ��� ã�� �����带 �ٽ� ������ �غ� �� ���
		///
		/// ����: AddWorkItem
		/// ����: threadSafeUpdateState
		/// ����: Update
		/// </summary>
		public bool ProcessWorkItems (bool force) {
			if (workItemsInProgressRightNow) throw new System.Exception("�۾� �׸��� ��������� ó�� ���Դϴ�. �۾� �׸� ������ �ٸ� �۾� �׸��� �Ϸ�Ǳ� ��ٸ��� ������. " +
			"�ڽ��� ��ũ��Ʈ���� �̰��� ������ �ƴ� ��� �̰��� ���� �� �� �ֽ��ϴ�.");

			UnityEngine.Physics2D.SyncTransforms();
			workItemsInProgressRightNow = true;
			astar.data.LockGraphStructure(true);
			while (workItems.Count > 0) {
				// ���ο� �ϰ� ó�� �۾� ��
				if (!workItemsInProgress) {
					workItemsInProgress = true;
					queuedWorkItemFloodFill = false;
				}

				// ť�� ù ��° �׸��� ������
				AstarWorkItem itm = workItems[0];
				bool status;

				try {
					// �׸��� ó�� ���� �� init ȣ��
					if (itm.init != null) {
						itm.init();
						itm.init = null;
					}

					if (itm.initWithContext != null) {
						itm.initWithContext(this);
						itm.initWithContext = null;
					}

					// ��⿭�� �׸��� �ֽ� �������� Ȯ��
					workItems[0] = itm;

					if (itm.update != null) {
						status = itm.update(force);
					} else if (itm.updateWithContext != null) {
						status = itm.updateWithContext(this, force);
					} else {
						status = true;
					}
				} catch {
					workItems.Dequeue();
					workItemsInProgressRightNow = false;
					astar.data.UnlockGraphStructure();
					throw;
				}

				if (!status) {
					if (force) {
						Debug.LogError("���������� �۾� �׸��Դϴ�. 'force'�� true������ �۾� �׸��� �Ϸ���� �ʾҽ��ϴ�.\n" +
					   "WorkItem�� 'force'�� true�� ���޵Ǹ� �׻� true�� ��ȯ�ؾ� �մϴ�.");
					}

					// ó���ؾ� �� �۾� �׸��� ���� ���� ����
					workItemsInProgressRightNow = false;
					astar.data.UnlockGraphStructure();
					return false;
				} else {
					workItems.Dequeue();
				}
			}

			EnsureValidFloodFill();

			Profiler.BeginSample("PostUpdate");
			if (anyGraphsDirty) GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
			Profiler.EndSample();

			anyGraphsDirty = false;
			workItemsInProgressRightNow = false;
			workItemsInProgress = false;
			astar.data.UnlockGraphStructure();
			return true;
		}
	}
}
