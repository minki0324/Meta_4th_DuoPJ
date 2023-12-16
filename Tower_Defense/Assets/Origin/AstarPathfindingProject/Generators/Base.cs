using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Serialization;

namespace Pathfinding {
	/// <summary>
	/// 그래프 내부 메서드를 노출시킵니다.
	/// 이것은 사용자 코드에서 사용해서는 안 되지만 여전히 'public' 또는 'internal' (이 라이브러리는 소스 코드와 함께 배포되므로 'public'과 거의 동일함)이어야 하는 메서드를 숨기기 위해 사용됩니다.
	///
	/// 내부 메서드를 숨기면 문서 및 IntelliSense 제안을 정리하는 데 도움이 됩니다.
	/// </summary>
	public interface IGraphInternals {
		string SerializedEditorSettings { get; set; }
		void OnDestroy();
		void DestroyAllNodes();
		IEnumerable<Progress> ScanInternal();
		void SerializeExtraInfo(GraphSerializationContext ctx);
		void DeserializeExtraInfo(GraphSerializationContext ctx);
		void PostDeserialization(GraphSerializationContext ctx);
		void DeserializeSettingsCompatibility(GraphSerializationContext ctx);
	}

	/// <summary>모든 그래프의 기본 클래스</summary>
	public abstract class NavGraph : IGraphInternals {
		/// <summary>씬 내 AstarPath 객체에 대한 참조</summary>
		public AstarPath active;

		/// <summary>
		/// 그래프의 ID로 고유하게 고려됩니다.
		/// 참고: 이것은 Pathfinding.Util.Guid이며 System.Guid 대신에 더 나은 iOS 호환성을 위해 코드화되었습니다.
		/// </summary>
		[JsonMember]
		public Guid guid;

		/// <summary>모든 노드에 적용할 기본 패널티</summary>
		[JsonMember]
		public uint initialPenalty;

		/// <summary>그래프가 에디터에서 열렸는지 여부</summary>
		[JsonMember]
		public bool open;

		/// <summary>그래프 식별을 위한 인덱스</summary>
		public uint graphIndex;

		/// <summary>
		/// 그래프의 이름입니다.
		/// 유니티 에디터에서 설정할 수 있습니다.
		/// </summary>
		[JsonMember]
		public string name;

		/// <summary>
		/// Unity 씬 뷰에서 기즈모스를 그릴 수 있게 하는 여부를 설정합니다.
		/// 인스펙터에서 이 값은 모든 그래프 인스펙터의 왼쪽 상단에있는 '눈' 아이콘의 상태에 해당합니다.
		/// </summary>
		[JsonMember]
		public bool drawGizmos = true;

		/// <summary>
		/// 에디터에서 정보 화면이 열려 있는지 확인하는 데 사용됩니다.
		/// UNITY_EDITOR 내부에서만 있어야합니다. 하지만 누군가가 Unity를 사용하여 NavGraph 인스턴스를 직렬화하려고 할 때를 대비하여 이렇게 남겨 두었습니다. 그렇지 않으면 빌드시 충돌을 일으킬 것이기 때문입니다. 이 버그 때문에 버전 3.0.8.1이 릴리스되었습니다.
		/// </summary>
		[JsonMember]
		public bool infoScreenOpen;

		/// <summary>그래프 인스펙터를위한 직렬화 된 설정을 저장하는 데 사용됩니다.</summary>
		[JsonMember]
		string serializedEditorSettings;


		/// <summary>그래프가 존재하는 경우 true, 삭제된 경우 false입니다.</summary>
		internal bool exists { get { return active != null; } }

		/// <summary>
		/// 그래프의 노드 수입니다.
		/// 이는 그래프 유형에 따라 오버라이드되지 않은 경우 O(n) 작업입니다.
		///
		/// 그리드 그래프 및 포인트 그래프의 경우 O(1) 작업입니다.
		/// 계층 그리드 그래프의 경우 O(n) 작업입니다.
		/// </summary>
		public virtual int CountNodes () {
			int count = 0;

			GetNodes(node => count++);
			return count;
		}

		/// <summary>델리게이트를 사용하여 그래프의 모든 노드를 호출합니다.</summary>
		public void GetNodes (System.Func<GraphNode, bool> action) {
			bool cont = true;

			GetNodes(node => {
				if (cont) cont &= action(node);
			});
		}

		/// <summary>
		/// 그래프의 모든 노드를 호출하는 데 사용됩니다.
		/// 이는 그래프의 모든 노드를 반복하는 주요 방법입니다.
		///
		/// 델리게이트 내에서 그래프 구조를 변경하지 마십시오.
		///
		/// 예:
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		///
		/// gg.GetNodes(node => {
		///     // 여기에 노드가 있습니다
		///     Debug.Log("노드를 찾았습니다. 위치: " + (Vector3)node.position);
		/// });
		/// </code>
		///
		/// 노드를 목록에 저장하려면 다음과 같이 할 수 있습니다.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		///
		/// List<GraphNode> nodes = new List<GraphNode>();
		///
		/// gg.GetNodes((System.Action<GraphNode>)nodes.Add);
		/// </code>
		///
		/// 참조: <see cref="Pathfinding.AstarData.GetNodes"/>
		/// </summary>
		public abstract void GetNodes(System.Action<GraphNode> action);

		/// <summary>
		/// 그래프를 변환/회전/크기 조정하는 데 사용되는 행렬입니다.
		/// 더 이상 사용되지 않는 메서드입니다. (일부 그래프 유형에서만 사용 가능)
		/// </summary>
		[System.Obsolete("Use the transform field (only available on some graph types) instead", true)]
		public Matrix4x4 matrix = Matrix4x4.identity;

		/// <summary>
		/// 행렬의 역입니다.
		/// 더 이상 사용되지 않는 메서드입니다. (일부 그래프 유형에서만 사용 가능)
		/// </summary>
		[System.Obsolete("Use the transform field (only available on some graph types) instead", true)]
		public Matrix4x4 inverseMatrix = Matrix4x4.identity;

		/// <summary>
		/// 행렬 및 역행렬을 동시에 설정하는 데 사용됩니다.
		/// 더 이상 사용되지 않는 메서드입니다. (일부 그래프 유형에서만 사용 가능)
		/// </summary>
		[System.Obsolete("Use the transform field (only available on some graph types) instead", true)]
		public void SetMatrix (Matrix4x4 m) {
			matrix = m;
			inverseMatrix = m.inverse;
		}

		/// <summary>
		/// 그래프의 노드를 이동합니다.
		/// 더 이상 사용되지 않는 메서드입니다. (일부 그래프 유형에서만 사용 가능)
		/// 대신 RelocateNodes(Matrix4x4)를 사용하세요.
		/// 원래의 위치를 복구하려면 RelocateNodes(newMatrix * oldMatrix.inverse)를 호출할 수 있습니다.
		/// </summary>
		[System.Obsolete("Use RelocateNodes(Matrix4x4) instead. To keep the same behavior you can call RelocateNodes(newMatrix * oldMatrix.inverse).")]
		public void RelocateNodes (Matrix4x4 oldMatrix, Matrix4x4 newMatrix) {
			RelocateNodes(newMatrix * oldMatrix.inverse);
		}

		/// <summary>
		/// 그래프의 노드를 이동합니다.
		/// 모든 노드 위치에 deltaMatrix를 곱합니다.
		///
		/// 예를 들어 모든 노드를 포인트 그래프에서 초기 위치에서 X 축을 따라 10단위로 이동하려면 다음과 같이 사용할 수 있습니다.
		/// <code>
		/// var graph = AstarPath.data.pointGraph;
		/// var m = Matrix4x4.TRS(new Vector3(10, 0, 0), Quaternion.identity, Vector3.one);
		/// graph.RelocateNodes(m);
		/// </code>
		///
		/// 참고: 그리드 그래프, 네비메쉬 그래프 및 리캐스트 그래프의 경우
		/// 중심 및 노드 크기 (및 추가 매개변수)를 사용하는 사용자 지정 RelocateNodes 메서드를 사용하는 것이 좋습니다.
		/// 더 쉽게 사용할 수 있으며 경로 찾기를 망치지 않을 가능성이 더 낮습니다.
		///
		/// 경고: 이 메서드는 PointGraphs에 대해서 정보를 손실합니다. 따라서 여러 번 호출하면 노드 위치가 정밀도를 잃을 수 있습니다.
		/// 예를 들어 한 호출에서 스케일을 0으로 설정하면 모든 노드가 동일한 지점으로 스케일/이동되어 원래 위치를 복구할 수 없게 됩니다.
		/// 다른 값에 대해서도 똑같이 작동하지만 덜 정도의 손실이 발생합니다.
		/// </summary>
		public virtual void RelocateNodes (Matrix4x4 deltaMatrix) {
			GetNodes(node => node.position = ((Int3)deltaMatrix.MultiplyPoint((Vector3)node.position)));
		}

		/// <summary>
		/// 위치에 가장 가까운 노드를 반환합니다.
		/// 참조: Pathfinding.NNConstraint.None
		/// </summary>
		/// <param name="position">가까운 노드를 찾으려는 위치</param>
		public NNInfoInternal GetNearest (Vector3 position) {
			return GetNearest(position, NNConstraint.None);
		}

		/// <summary>지정된 NNConstraint를 사용하여 위치에 가장 가까운 노드를 반환합니다.</summary>
		/// <param name="position">가까운 노드를 찾으려는 위치</param>
		/// <param name="constraint">예를 들어 걷기 가능한 노드를 반환하도록 지정할 수 있습니다. 좋은 노드를 얻지 못한 경우 GetNearestForce를 호출을 고려하세요.</param>
		public NNInfoInternal GetNearest (Vector3 position, NNConstraint constraint) {
			return GetNearest(position, constraint, null);
		}

		/// <summary>
		/// 지정된 NNConstraint를 사용하여 위치에 가장 가까운 노드를 반환합니다.
		/// </summary>
		/// <param name="position">가장 가까운 노드를 찾으려는 위치</param>
		/// <param name="hint">노드를 더 빨리 찾을 수 있도록 지정된 그래프 생성기를 활성화하는 데 사용할 수 있습니다.</param>
		/// <param name="constraint">예를 들어 걷기 가능한 노드를 반환하도록 함수에 지시할 수 있습니다. 적합한 노드를 얻지 못한 경우 GetNearestForce를 호출을 고려하세요.</param>
		public virtual NNInfoInternal GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
			// 이것은 기본 구현으로 꽤 느립니다.
			// 그래프는 일반적으로이 함수를 더 빠르고 특화 된 구현을 제공하기 위해 재정의합니다.

			float maxDistSqr = constraint == null || constraint.constrainDistance ? AstarPath.active.maxNearestNodeDistanceSqr : float.PositiveInfinity;

			float minDist = float.PositiveInfinity;
			GraphNode minNode = null;

			float minConstDist = float.PositiveInfinity;
			GraphNode minConstNode = null;

			// 모든 노드를 반복하고 가장 가까운 적합한 노드를 찾습니다.
			GetNodes(node => {
				float dist = (position-(Vector3)node.position).sqrMagnitude;

				if (dist < minDist) {
					minDist = dist;
					minNode = node;
				}

				if (dist < minConstDist && dist < maxDistSqr && (constraint == null || constraint.Suitable(node))) {
					minConstDist = dist;
					minConstNode = node;
				}
			});

			var nnInfo = new NNInfoInternal(minNode);

			nnInfo.constrainedNode = minConstNode;

			if (minConstNode != null) {
				nnInfo.constClampedPosition = (Vector3)minConstNode.position;
			} else if (minNode != null) {
				nnInfo.constrainedNode = minNode;
				nnInfo.constClampedPosition = (Vector3)minNode.position;
			}

			return nnInfo;
		}

		/// <summary>
		/// 제약 조건에 따라 위치에 가장 가까운 노드를 반환합니다.
		/// 반환값: NNInfo. 지정된 제약 조건을 충족하는 노드가 없는 경우에만 비어있는 NNInfo를 반환합니다.
		/// </summary>
		public virtual NNInfoInternal GetNearestForce (Vector3 position, NNConstraint constraint) {
			return GetNearest(position, constraint);
		}

		/// <summary>
		/// 참조를 정리하는 기능입니다.
		/// 이것은 AstarPath 스크립트가 연결된 gameObject에서 OnDisable와 동일한 시간에 호출됩니다 (기억하세요, 에디터에서는 아닙니다).
		/// 리소스가 수집되지 않게하는데 장애가되는 정적 변수를 정리하는 등의 정리 코드에 사용됩니다.
		/// 그래프의 모든 노드를 파괴해야합니다. 그렇지 않으면 메모리 누수가 발생합니다.
		/// </summary>
		protected virtual void OnDestroy () {
			DestroyAllNodes();
		}

		/// <summary>
		/// 그래프의 모든 노드를 파괴합니다.
		/// 경고: 이것은 내부 메서드입니다. 매우 좋은 이유가 없는 한 호출하지 않아야합니다.
		/// </summary>
		protected virtual void DestroyAllNodes () {
			GetNodes(node => node.Destroy());
		}

		/// <summary>
		/// 그래프를 스캔합니다.
		/// 더 이상 사용되지 않습니다. 대신 AstarPath.Scan()을 사용하세요.
		/// </summary>
		[System.Obsolete("Use AstarPath.Scan instead")]
		public void ScanGraph () {
			Scan();
		}

		/// <summary>
		/// 그래프를 스캔합니다.
		/// 모든 그래프를 동시에 스캔하는 것이 더 나은 경우가 많으므로이 함수 대신 AstarPath.Scan()을 사용하는 것이 좋습니다.
		/// </summary>
		public void Scan () {
			active.Scan(this);
		}

		/// <summary>
		/// 그래프를 스캔하는 내부 메서드입니다.
		/// AstarPath.ScanAsync에서 호출됩니다.
		/// 사용자 정의 스캔 논리를 구현하려면이 함수를 재정의하십시오.
		/// 진행 정보를 표시하고 비동기 스캔을 사용할 때 처리를 여러 프레임으로 나누려면 진행 개체를 생성하세요.
		/// </summary>
		protected abstract IEnumerable<Progress> ScanInternal();

		/// <summary>
		/// 그래프 유형별 노드 데이터를 직렬화합니다.
		/// 이 함수는 표준 직렬화를 사용하여 직렬화 할 수 없는 추가 노드 정보 (또는 그래프 정보)를 직렬화하기 위해 재정의 할 수 있습니다.
		/// 데이터를 원하는 방식으로 직렬화하고 바이트 배열을 반환하세요.
		/// 로드 할 때 정확히 동일한 바이트 배열이 DeserializeExtraInfo 함수로 전달됩니다.
		/// 이러한 함수는 노드 직렬화가 사용 중인 경우에만 호출됩니다.
		/// </summary>
		protected virtual void SerializeExtraInfo (GraphSerializationContext ctx) {
		}

		/// <summary>
		/// 그래프 유형별 노드 데이터를 역직렬화합니다.
		/// 참조: SerializeExtraInfo
		/// </summary>
		protected virtual void DeserializeExtraInfo (GraphSerializationContext ctx) {
		}

		/// <summary>
		/// 모든 그래프의 모든 스캔이 완료된 후에 호출됩니다.
		/// 직렬화되지 않은 그래프 데이터를 설정하는 데 사용할 수 있습니다.
		/// </summary>
		protected virtual void PostDeserialization (GraphSerializationContext ctx) {
		}

		/// <summary>
		/// 설정을 직렬화하는 ​​옛날 형식입니다.
		/// 사용되지 않는 방식입니다. 그러나 deserialization 코드는 업그레이드 중에 데이터를 잃어 버리지 않기 위해 유지됩니다.
		/// </summary>
		protected virtual void DeserializeSettingsCompatibility (GraphSerializationContext ctx) {
			guid = new Guid(ctx.reader.ReadBytes(16));
			initialPenalty = ctx.reader.ReadUInt32();
			open = ctx.reader.ReadBoolean();
			name = ctx.reader.ReadString();
			drawGizmos = ctx.reader.ReadBoolean();
			infoScreenOpen = ctx.reader.ReadBoolean();
		}

		/// <summary>그래프를 위한 gizmo를 그립니다.</summary>
		public virtual void OnDrawGizmos (RetainedGizmos gizmos, bool drawNodes) {
			if (!drawNodes) {
				return;
			}

			// 이것은 상대적으로 느린 기본 구현입니다.
			// 기본 그래프 클래스의 하위 클래스가이 메서드를 재정의하여 더 최적화된 방식으로 gizmo를 그릴 수 있습니다.


			var hasher = new RetainedGizmos.Hasher(active);
			GetNodes(node => hasher.HashNode(node));

			// Update the gizmo mesh if necessary
			if (!gizmos.Draw(hasher)) {
				using (var helper = gizmos.GetGizmoHelper(active, hasher)) {
					GetNodes((System.Action<GraphNode>)helper.DrawConnections);
				}
			}

			if (active.showUnwalkableNodes) DrawUnwalkableNodes(active.unwalkableNodeDebugSize);
		}

		protected void DrawUnwalkableNodes (float size) {
			Gizmos.color = AstarColor.UnwalkableNode;
			GetNodes(node => {
				if (!node.Walkable) Gizmos.DrawCube((Vector3)node.position, Vector3.one*size);
			});
		}

		#region IGraphInternals implementation
		string IGraphInternals.SerializedEditorSettings { get { return serializedEditorSettings; } set { serializedEditorSettings = value; } }
		void IGraphInternals.OnDestroy () { OnDestroy(); }
		void IGraphInternals.DestroyAllNodes () { DestroyAllNodes(); }
		IEnumerable<Progress> IGraphInternals.ScanInternal () { return ScanInternal(); }
		void IGraphInternals.SerializeExtraInfo (GraphSerializationContext ctx) { SerializeExtraInfo(ctx); }
		void IGraphInternals.DeserializeExtraInfo (GraphSerializationContext ctx) { DeserializeExtraInfo(ctx); }
		void IGraphInternals.PostDeserialization (GraphSerializationContext ctx) { PostDeserialization(ctx); }
		void IGraphInternals.DeserializeSettingsCompatibility (GraphSerializationContext ctx) { DeserializeSettingsCompatibility(ctx); }

		#endregion
	}


	/// <summary>
	/// 그래프의 충돌 검사를 처리합니다.
	/// 주로 그리드 기반 그래프에서 사용됩니다.
	/// </summary>
	[System.Serializable]
	public class GraphCollision {
		/// <summary>
		/// 사용할 충돌 모양입니다.
		/// 참조: <see cref="Pathfinding.ColliderType"/>
		/// </summary>
		public ColliderType type = ColliderType.Capsule;

		/// <summary>
		/// 충돌 검사 시 캡슐 또는 구의 지름입니다.
		/// 충돌을 확인할 때 시스템은 노드 위치에서 특정 모양과의 겹침 여부를 확인합니다. 모양은 <see cref="type"/> 필드로 결정됩니다.
		///
		/// 지름이 1이면 모양의 지름이 노드의 너비와 같거나, 다른 말로하면 <link Pathfinding.GridGraph.nodeSize nodeSize </link>와 같습니다.
		///
		/// <see cref="type"/>이 Ray로 설정된 경우 아무런 영향을 미치지 않습니다.
		///
		/// [온라인 문서에서 이미지 확인]
		/// </summary>
		public float diameter = 1F;

		/// <summary>
		/// 충돌 검사 시 캡슐 또는 레이의 길이입니다.
		/// <see cref="type"/>이 Sphere로 설정된 경우 아무런 영향을 미치지 않습니다.
		///
		/// 경고: Unity의 캡슐 콜라이더 및 캐릭터 컨트롤러와 달리 이 높이는 캡슐의 끝 구를 포함하지 않고 실린더 부분만 포함합니다.
		/// 이것은 주로 역사적인 이유 때문입니다.
		/// </summary>
		public float height = 2F;

		/// <summary>
		/// 충돌 검사에 사용할 때 지면 상단부터 높이까지의 거리입니다.
		/// 예를 들어, 지면이 y=0에서 찾아졌다면 collisionOffset = 2, type = Capsule 및 height = 3으로 설정되었다면
		/// 물리 시스템은 y=2에서 중심에 있는 하단 구로 구성된 캡슐 내에 어떤 콜라이더가 있는지 쿼리합니다.
		///
		/// type = Sphere인 경우 경우에는 구의 중심이 여기에서 y=2가됩니다.
		/// </summary>
		public float collisionOffset;

		/// <summary>
		/// 충돌 검사 시 레이의 방향입니다.
		/// <see cref="type"/>이 Ray가 아닌 경우 아무런 영향을 미치지 않습니다.
		/// </summary>
		public RayDirection rayDirection = RayDirection.Both;

		/// <summary>장애물로 처리 할 레이어</summary>
		public LayerMask mask;

		/// <summary>높이 검사에 포함될 레이어</summary>
		public LayerMask heightMask = -1;

		/// <summary>
		/// 높이 검사할 때 사용할 높이입니다 ('레이 길이'는 인스펙터에서 표시됨).
		///
		/// 아래 이미지와 같이 다른 레이 길이는 레이가 다른 것에 부딪치게 만들 수 있습니다.
		/// 거리는 그래프 평면에서 측정됩니다.
		///
		/// [온라인 문서에서 이미지 확인]
		/// </summary>
		public float fromHeight = 100;

		/// <summary>
		/// 두꺼운 레이캐스트를 토글합니다.
		/// 참조: https://docs.unity3d.com/ScriptReference/Physics.SphereCast.html
		/// </summary>
		public bool thickRaycast;

		/// <summary>
		/// 노드에서 두꺼운 레이캐스트의 지름입니다.
		/// 1은 \link Pathfinding.GridGraph.nodeSize nodeSize \endlink와 동일합니다.
		/// </summary>
		public float thickRaycastDiameter = 1;

		/// <summary>높이 레이캐스트에서 지면을 찾지 못했을 때 노드를 걷지 못하게 만듭니다. 높이 레이캐스트가 꺼져 있으면 아무런 영향을 미치지 않습니다.</summary>
		public bool unwalkableWhenNoGround = true;

		/// <summary>
		/// Unity 2D 물리 API를 사용할지 여부를 토글합니다.
		/// 참조: http://docs.unity3d.com/ScriptReference/Physics2D.html
		/// </summary>
		public bool use2D;

		/// <summary>충돌 검사를 토글합니다.</summary>
		public bool collisionCheck = true;

		/// <summary>높이 검사를 토글합니다. false인 경우 그리드는 평평합니다.</summary>
		public bool heightCheck = true;

		/// <summary>UP으로 사용할 방향을 결정합니다.</summary>
		public Vector3 up;

		/// <summary><see cref="up"/> * <see cref="height"/>.</summary>
		private Vector3 upheight;

		/// <summary>2D 충돌 쿼리에 사용됩니다.</summary>
		private ContactFilter2D contactFilter;

		/// <summary>
		/// Physics2D.OverlapPoint 메서드에 일부 버퍼를 저장할 공간이 있도록 만듭니다.
		/// 실제로이 배열에서 읽지 않으므로 이게 스레드 안전한지 신경 쓰지 않습니다.
		/// </summary>
		private static Collider2D[] dummyArray = new Collider2D[1];

		/// <summary>
		/// <see cref="diameter"/> * scale * 0.5입니다.
		/// 일반적으로 scale은 \link Pathfinding.GridGraph.nodeSize nodeSize \endlink입니다.
		/// </summary>
		private float finalRadius;

		/// <summary>
		/// <see cref="thickRaycastDiameter"/> * scale * 0.5입니다.
		/// 일반적으로 scale은 \link Pathfinding.GridGraph.nodeSize nodeSize \endlink입니다.
		/// </summary>
		private float finalRaycastRadius;

		/// <summary>CheckHeightAll에서 동일한 지점을 다시 명중하지 않도록 각 레이캐스트 후에 적용할 오프셋입니다.</summary>
		public const float RaycastErrorMargin = 0.005F;

		/// <summary>
		/// 지정된 행렬 및 스케일을 사용하여 여러 변수를 설정합니다.
		/// 참조: GraphCollision.up
		/// 참조: GraphCollision.upheight
		/// 참조: GraphCollision.finalRadius
		/// 참조: GraphCollision.finalRaycastRadius
		/// </summary>
		public void Initialize (GraphTransform transform, float scale) {
			up = (transform.Transform(Vector3.up) - transform.Transform(Vector3.zero)).normalized;
			upheight = up*height;
			finalRadius = diameter*scale*0.5F;
			finalRaycastRadius = thickRaycastDiameter*scale*0.5F;
			contactFilter = new ContactFilter2D { layerMask = mask, useDepth = false, useLayerMask = true, useNormalAngle = false, useTriggers = false };
		}

		/// <summary>
		/// 위치가 장애물로 막히지 않았는지 여부를 반환합니다.
		/// <see cref="collisionCheck"/>가 false인 경우 항상 true를 반환합니다.
		/// </summary>
		public bool Check (Vector3 position) {
			if (!collisionCheck) {
				return true;
			}

			if (use2D) {
				switch (type) {
				case ColliderType.Capsule:
				case ColliderType.Sphere:
					return Physics2D.OverlapCircle(position, finalRadius, contactFilter, dummyArray) == 0;
				default:
					return Physics2D.OverlapPoint(position, contactFilter, dummyArray) == 0;
				}
			}

			position += up*collisionOffset;
			switch (type) {
			case ColliderType.Capsule:
				return !Physics.CheckCapsule(position, position+upheight, finalRadius, mask, QueryTriggerInteraction.Ignore);
			case ColliderType.Sphere:
				return !Physics.CheckSphere(position, finalRadius, mask, QueryTriggerInteraction.Ignore);
			default:
				switch (rayDirection) {
				case RayDirection.Both:
					return !Physics.Raycast(position, up, height, mask, QueryTriggerInteraction.Ignore) && !Physics.Raycast(position+upheight, -up, height, mask, QueryTriggerInteraction.Ignore);
				case RayDirection.Up:
					return !Physics.Raycast(position, up, height, mask, QueryTriggerInteraction.Ignore);
				default:
					return !Physics.Raycast(position+upheight, -up, height, mask, QueryTriggerInteraction.Ignore);
				}
			}
		}

		/// <summary>
		/// 올바른 높이로 위치를 반환합니다.
		/// <see cref="heightCheck"/>가 false인 경우 position을 반환합니다.
		/// </summary>
		public Vector3 CheckHeight (Vector3 position) {
			RaycastHit hit;
			bool walkable;

			return CheckHeight(position, out hit, out walkable);
		}

		/// <summary>
		/// 올바른 높이로 위치를 반환합니다.
		/// <see cref="heightCheck"/>가 false인 경우 position을 반환합니다.
		/// walkable이라는 out 매개 변수가 없으면 아무것도 맞추지 않은 경우 false로 설정됩니다.
		/// 지면이 그리드의 기본 부분에 정확히 있을 때 부동 소수점 오류를 피하기 위해 레이를 그리드의 기반 아래로 조금 더 그립니다.
		/// </summary>
		public Vector3 CheckHeight (Vector3 position, out RaycastHit hit, out bool walkable) {
			walkable = true;

			if (!heightCheck || use2D) {
				hit = new RaycastHit();
				return position;
			}

			if (thickRaycast) {
				var ray = new Ray(position+up*fromHeight, -up);
				if (Physics.SphereCast(ray, finalRaycastRadius, out hit, fromHeight+0.005F, heightMask, QueryTriggerInteraction.Ignore)) {
					return VectorMath.ClosestPointOnLine(ray.origin, ray.origin+ray.direction, hit.point);
				}

				walkable &= !unwalkableWhenNoGround;
			} else {
				// 위에서 아래로 레이를 쏘아 지면을 찾으려고 시도
				if (Physics.Raycast(position+up*fromHeight, -up, out hit, fromHeight+0.005F, heightMask, QueryTriggerInteraction.Ignore)) {
					return hit.point;
				}

				walkable &= !unwalkableWhenNoGround;
			}
			return position;
		}

		/// <summary><see cref="CheckHeightAll"/>에서 사용하는 내부 버퍼</summary>
		RaycastHit[] hitBuffer = new RaycastHit[8];

		/// <summary>
		/// 위치에 대한 높이 확인 시 모든 충돌을 반환합니다.
		/// 경고: 두꺼운 레이캐스트와는 잘 작동하지 않으며, 객체를 한 번만 반환합니다.
		///
		/// 경고: 반환된 배열은 순간적입니다. 이 메서드가 다시 호출될 때 무효화됩니다.
		/// 영구적인 결과가 필요한 경우 복사해야 합니다.
		///
		/// 반환된 배열은 실제 충돌 수보다 큰 경우가 있으며, numHits 출력 매개변수는 실제 충돌 수를 나타냅니다.
		/// </summary>
		public RaycastHit[] CheckHeightAll (Vector3 position, out int numHits) {
			if (!heightCheck || use2D) {
				hitBuffer[0] = new RaycastHit {
					point = position,
					distance = 0,
				};
				numHits = 1;
				return hitBuffer;
			}

			// 위에서 아래로 레이를 쏴서 지면을 찾으려고 시도합니다.
#if UNITY_2017_1_OR_NEWER
			numHits = Physics.RaycastNonAlloc(position+up*fromHeight, -up, hitBuffer, fromHeight+0.005F, heightMask, QueryTriggerInteraction.Ignore);
			if (numHits == hitBuffer.Length) {
				// 더 큰 버퍼로 다시 시도합니다.
				hitBuffer = new RaycastHit[hitBuffer.Length*2];
				return CheckHeightAll(position, out numHits);
			}
			return hitBuffer;
#else
			var result = Physics.RaycastAll(position+up*fromHeight, -up, fromHeight+0.005F, heightMask, QueryTriggerInteraction.Ignore);
			numHits = result.Length;
			return result;
#endif
		}

		public void DeserializeSettingsCompatibility (GraphSerializationContext ctx) {
			type = (ColliderType)ctx.reader.ReadInt32();
			diameter = ctx.reader.ReadSingle();
			height = ctx.reader.ReadSingle();
			collisionOffset = ctx.reader.ReadSingle();
			rayDirection = (RayDirection)ctx.reader.ReadInt32();
			mask = (LayerMask)ctx.reader.ReadInt32();
			heightMask = (LayerMask)ctx.reader.ReadInt32();
			fromHeight = ctx.reader.ReadSingle();
			thickRaycast = ctx.reader.ReadBoolean();
			thickRaycastDiameter = ctx.reader.ReadSingle();

			unwalkableWhenNoGround = ctx.reader.ReadBoolean();
			use2D = ctx.reader.ReadBoolean();
			collisionCheck = ctx.reader.ReadBoolean();
			heightCheck = ctx.reader.ReadBoolean();
		}
	}


	/// <summary>
	/// 충돌 검사 모양을 결정합니다.
	/// 참조: <see cref="Pathfinding.GraphCollision"/>
	/// </summary>
	public enum ColliderType
	{
		/// <summary>구를 사용합니다. Physics.CheckSphere에서 사용됩니다. 2D에서는 원 형태입니다.</summary>
		Sphere,
		/// <summary>캡슐을 사용합니다. Physics.CheckCapsule에서 사용됩니다. 2D에서는 Sphere 모드와 동일하게 작동합니다.</summary>
		Capsule,
		/// <summary>레이를 사용합니다. Physics.Linecast에서 사용됩니다. 2D에서는 점 하나를 나타냅니다.</summary>
		Ray
	}

	/// <summary>충돌 검사 레이 방향을 결정합니다</summary>
	public enum RayDirection
	{
		Up,     /// <summary>아래에서 위로 레이를 쏩니다.</summary>
		Down,   /// <summary>위에서 아래로 레이를 쏩니다.</summary>
		Both    /// <summary>양쪽 방향으로 두 개의 레이를 쏩니다.</summary>
	}
}
