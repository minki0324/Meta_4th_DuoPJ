using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	using UnityEngine;

	/// <summary>
	/// 그래프가 업데이트될 때 실행할 수 있는 작업 항목입니다.
	/// 참조: <see cref="AstarPath.UpdateGraphs"/>
	/// 참조: <see cref="AstarPath.AddWorkItem"/>
	/// </summary>
	public struct AstarWorkItem {
		/// <summary>
		/// 초기화 함수.
		/// 초기화가 필요하지 않은 경우 null일 수 있습니다.
		/// <see cref="update"/> 호출 직전에 한 번 호출됩니다.
		/// </summary>
		public System.Action init;

		/// <summary>
		/// 초기화 함수.
		/// 초기화가 필요하지 않은 경우 null일 수 있습니다.
		/// <see cref="update"/> 호출 직전에 한 번 호출됩니다.
		///
		/// 컨텍스트 객체가 매개변수로 전달됩니다. 이를 사용하여 예를 들어 홍수 채우기(flood fill)를 대기열에 넣을 수 있으며,
		/// 홍수 채우기는 작업 항목이 <see cref="EnsureValidFloodFill"/>을 호출하거나 모든 작업 항목이 완료될 때 실행됩니다.
		/// 노드를 업데이트하는 여러 작업 항목이 홍수 채우기를 필요로 하는 경우 <see cref="QueueFloodFill"/> 메서드를 사용하는 것이 좋습니다.
		/// 작업 항목마다 홍수 채우기를 수행하는 대신 모든 작업 항목에 대해 하나의 홍수 채우기만 수행되기 때문입니다.
		/// </summary>
		public System.Action<IWorkItemContext> initWithContext;

		/// <summary>
		/// 작업 항목이 실행될 때 한 번 프레임당 호출되는 업데이트 함수입니다.
		/// force 매개변수를 취합니다. true인 경우 작업 항목은 여러 프레임에 걸쳐 퍼지지 않고 한 번에 완료하려고 시도해야 합니다.
		/// 반환값: 작업 항목이 완료되면 true.
		/// </summary>
		public System.Func<bool, bool> update;

		/// <summary>
		/// 작업 항목이 실행될 때 한 번 프레임당 호출되는 업데이트 함수입니다.
		/// force 매개변수를 취합니다. true인 경우 작업 항목은 여러 프레임에 걸쳐 퍼지지 않고 한 번에 완료하려고 시도해야 합니다.
		/// 반환값: 작업 항목이 완료되면 true.
		///
		/// 컨텍스트 객체가 매개변수로 전달됩니다. 이를 사용하여 예를 들어 홍수 채우기(flood fill)를 대기열에 넣을 수 있으며,
		/// 홍수 채우기는 작업 항목이 <see cref="EnsureValidFloodFill"/>을 호출하거나 모든 작업 항목이 완료될 때 실행됩니다.
		/// 노드를 업데이트하는 여러 작업 항목이 홍수 채우기를 필요로 하는 경우 <see cref="QueueFloodFill"/> 메서드를 사용하는 것이 좋습니다.
		/// 작업 항목마다 홍수 채우기를 수행하는 대신 모든 작업 항목에 대해 하나의 홍수 채우기만 수행되기 때문입니다.
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

	/// <summary>WorkItemProcessor 기능의 일부를 노출하는 인터페이스</summary>
	public interface IWorkItemContext
	{
		/// <summary>
		/// 작업 항목 내에서 호출하여 홍수 채우기(flood fill)를 대기열에 추가합니다.
		/// 홍수 채우기를 즉시 수행하려면 FloodFill()을 사용할 수 있지만,
		/// 이 메서드를 사용하여 여러 업데이트를 하나로 묶어 성능을 향상시킬 수 있습니다.
		/// 작업 항목이 실행 중에 유효한 홍수 채우기가 필요한 경우 <see cref="EnsureValidFloodFill"/>을 호출하여
		/// 대기열에 홍수 채우기가 추가되었는지 확인할 수 있습니다.
		///
		/// 홍수 채우기가 대기열에 추가되면 모든 작업 항목이 실행된 후에 수행됩니다.
		///
		/// 사용 불가: 사용하지 않는 것이 좋습니다. 이 메서드를 사용하면 연결 구성 요소의 완전한 재계산이 강제됩니다. 대부분의 경우 계층 그래프 클래스가 뒷단에서 작업을 자동으로 처리하므로 이 함수 호출을 제거할 수 있어야 합니다.
		/// </summary>
		[System.Obsolete("사용을 피하세요. 이 메서드를 사용하면 연결된 구성 요소의 완전한 재계산이 강제됩니다. 대부분의 경우 계층 그래프 클래스가 뒷단에서 작업을 자동으로 처리하므로 이 함수 호출을 제거할 수 있어야 합니다.")]
		void QueueFloodFill();

		/// <summary>
		/// 작업 항목이 실행 중에 유효한 영역 정보가 필요한 경우 사용하여 대기열에 홍수 채우기(flood fill)가 없음을 확인합니다.
		/// 작업 항목에서 <see cref="Pathfinding.GraphNode.Area"/> 속성 또는 <see cref="Pathfinding.PathUtilities.IsPathPossible"/> 메서드를 사용하는 경우
		/// 사용하기 전에 이 메서드를 호출하여 데이터가 최신 상태인지 확인할 수 있습니다.
		///
		/// 참조: <see cref="Pathfinding.HierarchicalGraph"/>
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(new AstarWorkItem((IWorkItemContext ctx) => {
		///     ctx.EnsureValidFloodFill();
		///
		///     // 위의 호출은 이 메서드가 그래프에 대한 최신 정보를 가지고 있는 것을 보장합니다
		///     if (PathUtilities.IsPathPossible(someNode, someOtherNode)) {
		///         // 무언가를 수행합니다
		///     }
		/// }));
		/// </code>
		/// </summary>
		void EnsureValidFloodFill();

		/// <summary>
		/// 그래프 수정 이벤트를 트리거합니다.
		/// 이로 인해 <see cref="Pathfinding.GraphModifier.PostUpdate"/> 이벤트가 모든 그래프 업데이트가 완료된 후에 발생합니다.
		/// 일부 스크립트는 이 이벤트를 청취합니다. 예를 들어 오프-메시 링크는 청취하고 있으며 이 이벤트가 전송될 때 연결된 노드를 다시 계산합니다.
		/// 그래프가 여러 번 수정되거나 여러 개의 그래프가 수정된 경우 이벤트는 한 번만 전송됩니다.
		/// </summary>
		void SetGraphDirty(NavGraph graph);
	}

	class WorkItemProcessor : IWorkItemContext {
		/// <summary>작업 항목이 다른 작업 항목 내에서 완료를 대기하는 것을 방지하기 위해 사용됩니다. 이로 인해 프로그램이 멈출 수 있습니다.</summary>
		public bool workItemsInProgressRightNow { get; private set; }

		readonly AstarPath astar;
		readonly IndexedQueue<AstarWorkItem> workItems = new IndexedQueue<AstarWorkItem>();

		/// <summary>현재 대기 중인 작업 항목이 있는 경우 true</summary>
		public bool anyQueued {
			get { return workItems.Count > 0; }
		}

		/// <summary>
		/// 현재 작업 항목이 홍수 채우기를 대기 중인 경우 true입니다.
		/// 참조: QueueWorkItemFloodFill
		/// </summary>
		bool queuedWorkItemFloodFill = false;

		bool anyGraphsDirty = true;

		/// <summary>
		/// 작업 항목 일괄 처리 중일 때 true입니다.
		/// 작업 항목 업데이트는 종종 여러 프레임에 걸쳐 분산되므로이 플래그는 업데이트가 진행 중인 동안 항상 true입니다.
		/// </summary>
		public bool workItemsInProgress { get; private set; }

		/// <summary>Queue<T>와 유사하지만 무작위 액세스를 허용합니다</summary>
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
		/// 작업 항목 실행 중에 홍수 채우기를 대기열에 추가하려면 호출합니다.
		/// 즉시 홍수 채우기는 FloodFill()을 통해 수행할 수 있지만 여러 업데이트를 하나로 묶어 성능을 향상시키기 위해 이 메서드를 사용할 수 있습니다.
		/// 실행 중인 작업 항목이 이전에 홍수 채우기를 대기열에 추가한 경우, 모든 작업 항목이 실행된 후에 홍수 채우기가 수행됩니다.
		/// </summary>
		void IWorkItemContext.QueueFloodFill () {
			queuedWorkItemFloodFill = true;
		}

		void IWorkItemContext.SetGraphDirty (NavGraph graph) {
			anyGraphsDirty = true;
		}

		/// <summary>작업 항목이 실행 중에 유효한 영역 정보가 필요한 경우 호출하여 대기 중인 홍수 채우기가 없음을 확인합니다.</summary>
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
		/// 작업 항목을 추가하여 경로 찾기가 일시 중지될 때 처리되도록합니다.
		/// 참조: ProcessWorkItems
		/// </summary>
		public void AddWorkItem(AstarWorkItem item)
		{
			workItems.Enqueue(item);
		}

		/// <summary>
		/// 그래프 업데이트 작업 항목을 처리합니다.
		/// 모든 대기 중인 작업 항목을 처리합니다. 예를 들어 그래프 업데이트 등을 처리합니다.
		///
		/// 반환값:
		/// - false: 아직 처리해야 할 항목이 있을 경우
		/// - true: 마지막 작업 항목이 처리되고 경로 찾기 스레드를 다시 시작할 준비가 된 경우
		///
		/// 참조: AddWorkItem
		/// 참조: threadSafeUpdateState
		/// 참조: Update
		/// </summary>
		public bool ProcessWorkItems (bool force) {
			if (workItemsInProgressRightNow) throw new System.Exception("작업 항목을 재귀적으로 처리 중입니다. 작업 항목 내에서 다른 작업 항목이 완료되길 기다리지 마세요. " +
			"자신의 스크립트에서 이것이 원인이 아닌 경우 이것은 버그 일 수 있습니다.");

			UnityEngine.Physics2D.SyncTransforms();
			workItemsInProgressRightNow = true;
			astar.data.LockGraphStructure(true);
			while (workItems.Count > 0) {
				// 새로운 일괄 처리 작업 중
				if (!workItemsInProgress) {
					workItemsInProgress = true;
					queuedWorkItemFloodFill = false;
				}

				// 큐의 첫 번째 항목을 엿보기
				AstarWorkItem itm = workItems[0];
				bool status;

				try {
					// 항목이 처음 보일 때 init 호출
					if (itm.init != null) {
						itm.init();
						itm.init = null;
					}

					if (itm.initWithContext != null) {
						itm.initWithContext(this);
						itm.initWithContext = null;
					}

					// 대기열의 항목이 최신 상태인지 확인
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
						Debug.LogError("비정상적인 작업 항목입니다. 'force'가 true이지만 작업 항목이 완료되지 않았습니다.\n" +
					   "WorkItem에 'force'가 true로 전달되면 항상 true를 반환해야 합니다.");
					}

					// 처리해야 할 작업 항목이 아직 남아 있음
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
