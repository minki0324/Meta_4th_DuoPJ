using UnityEngine;
using System.Collections.Generic;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	/// <summary>
	/// 단일 유닛의 경로 호출을 처리합니다.
	/// \ingroup relevant
	/// 이 컴포넌트는 단일 유닛 (AI, 로봇, 플레이어 등)에 부착되어 경로 탐색 호출을 처리하고 경로 수정기를 사용하여 경로의 후 처리를 담당합니다.
	///
	/// [이미지를 보려면 온라인 설명서를 열어보세요]
	///
	/// 참조: calling-pathfinding (온라인 설명서에서 작동하는 링크로 확인)
	/// 참조: modifiers (온라인 설명서에서 작동하는 링크로 확인)
	/// </summary>
	[AddComponentMenu("Pathfinding/Seeker")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_seeker.php")]
	public class Seeker : VersionedMonoBehaviour {
		/// <summary>
		/// 마지막 계산된 경로를 Gizmos를 사용하여 그립니다.
		/// 경로는 녹색으로 표시됩니다.
		///
		/// 참조: OnDrawGizmos
		/// </summary>
		public bool drawGizmos = true;

		/// <summary>
		/// 후 처리되지 않은 경로를 Gizmos를 사용하여 그릴지 여부를 활성화합니다.
		/// 경로는 주황색으로 표시됩니다.
		///
		/// <see cref="drawGizmos"/>가 true 여야 합니다.
		///
		/// 이것은 경로에 부드러운 처리와 같은 후 처리가 적용되기 전의 경로를 표시합니다.
		///
		/// 참조: drawGizmos
		/// 참조: OnDrawGizmos
		/// </summary>
		public bool detailedGizmos;

		/// <summary>
		/// 경로의 시작점과 끝점을 조정하는 경로 수정기입니다.
		/// </summary>
		[HideInInspector]
		public StartEndModifier startEndModifier = new StartEndModifier();

		/// <summary>
		/// Seeker가 탐색할 수 있는 태그입니다.
		///
		/// 참고: 이 필드는 비트마스크(bitmask)입니다.
		/// 비트마스크에 대한 자세한 내용은 온라인 설명서를 참조하십시오.
		/// </summary>
		[HideInInspector]
		public int traversableTags = -1;

		/// <summary>
		/// 각 태그에 대한 패널티 값입니다.
		/// 기본 태그인 태그 0은 tagPenalties[0]에 패널티가 추가됩니다.
		/// 이 값들은 A* 알고리즘이 음수 패널티를 처리할 수 없으므로 양수 값만 사용해야 합니다.
		///
		/// 참고: 이 배열의 길이는 항상 32여야 하며, 그렇지 않으면 시스템에서 무시됩니다.
		///
		/// 참조: Pathfinding.Path.tagPenalties
		/// </summary>
		[HideInInspector]
		public int[] tagPenalties = new int[32];

		/// <summary>
		/// 이 Seeker가 사용할 수 있는 그래프입니다.
		/// 이 필드는 경로의 시작점과 끝점을 검색할 때 어떤 그래프를 고려할지 결정합니다.
		/// 이것은 다양한 상황에서 유용하며, 작은 단위용 그래프와 큰 단위용 그래프를 만들고 싶을 때 사용할 수 있습니다.
		///
		/// 이것은 비트마스크이므로, 예를 들어 에이전트가 그래프 인덱스 3만 사용하려면 다음과 같이 설정할 수 있습니다:
		/// <code> seeker.graphMask = 1 << 3; </code>
		///
		/// 참조: bitmasks (온라인 설명서에서 작동하는 링크로 확인)
		///
		/// 이 필드는 허용된 그래프 인덱스만 저장합니다. 그래프의 순서가 변경되면 이 마스크가 더 이상 올바르지 않을 수 있습니다.
		///
		/// 그래프의 이름을 알고 있다면 <see cref="Pathfinding.GraphMask.FromGraphName"/> 메서드를 사용할 수 있습니다:
		/// <code>
		/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
		/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
		///
		/// NNConstraint nn = NNConstraint.Default;
		///
		/// nn.graphMask = mask1 | mask2;
		///
		/// // 'My Grid Graph' 또는 'My Other Grid Graph'에 속하는 somePoint 가장 가까운 노드를 찾습니다.
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		///
		/// 일부 <see cref="StartPath"/> 메서드의 오버로드는 graphMask 매개변수를 사용합니다.
		/// 이러한 오버로드가 사용되면 해당 경로 요청의 그래프 마스크를 재정의합니다.
		///
		/// [이미지를 보려면 온라인 설명서를 열어보세요]
		///
		/// 참조: multiple-agent-types (온라인 설명서에서 작동하는 링크로 확인)
		/// </summary>
		[HideInInspector]
		public GraphMask graphMask = GraphMask.everything;

		/// <summary>이전 버전의 직렬화 호환성을 위해 사용됩니다</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("graphMask")]
		int graphMaskCompatibility = -1;

		/// <summary>
		/// 경로가 완료될 때 호출되는 콜백입니다.
		/// 이 델리게이트는 움직임 스크립트가 등록해야 합니다.
		/// StartPath를 호출할 때 임시 콜백도 설정할 수 있지만, 해당 경로에 대해서만 호출됩니다.
		/// </summary>
		public OnPathDelegate pathCallback;

		/// <summary>경로 탐색이 시작되기 전에 호출됩니다</summary>
		public OnPathDelegate preProcessPath;

		/// <summary>경로가 계산된 후, 수정기가 실행되기 직전에 호출됩니다.</summary>
		public OnPathDelegate postProcessPath;

		/// <summary>Gizmos를 그리기 위해 사용됩니다</summary>
		[System.NonSerialized]
		List<Vector3> lastCompletedVectorPath;

		/// <summary>Gizmos를 그리기 위해 사용됩니다</summary>
		[System.NonSerialized]
		List<GraphNode> lastCompletedNodePath;

		/// <summary>현재 경로입니다</summary>
		[System.NonSerialized]
		protected Path path;

		/// <summary>이전 경로. Gizmos를 그리기 위해 사용됩니다</summary>
		[System.NonSerialized]
		private Path prevPath;

		/// <summary>경로가 시작될 때 매번 할당을 피하기 위한 캐시된 델리게이트</summary>
		private readonly OnPathDelegate onPathDelegate;

		/// <summary>현재 경로에만 호출되는 임시 콜백입니다. 이 값은 StartPath 함수에 의해 설정됩니다</summary>
		private OnPathDelegate tmpPathCallback;

		/// <summary>최근 경로 조회의 경로 ID</summary>
		protected uint lastPathID;

		/// <summary>모든 수정기의 내부 목록</summary>
		readonly List<IPathModifier> modifiers = new List<IPathModifier>();

		public enum ModifierPass {
			PreProcess,
			// 이전에 사용되던 항목이 인덱스 1을 점유했습니다.
			PostProcess = 2,
		}

		public Seeker () {
			onPathDelegate = OnPathComplete;
		}

		/// <summary>몇 가지 변수를 초기화합니다.</summary>
		protected override void Awake () {
			base.Awake();
			startEndModifier.Awake(this);
		}

		/// <summary>
		/// 현재 계산 중인 경로 또는 마지막으로 계산한 경로입니다.
		/// 이 메서드를 사용하는 것은 드물며, 대신 경로 콜백이 호출될 때 경로를 가져와야 합니다.
		///
		/// 참조: pathCallback
		/// </summary>
		public Path GetCurrentPath()
		{
			return path;
		}

		/// <summary>
		/// 현재 경로 요청을 계산 중지합니다.
		/// 이 Seeker가 현재 경로를 계산 중인 경우 경로 계산이 취소됩니다.
		/// 콜백(일반적으로 OnPathComplete라는 메서드)은 경로의 'error' 필드가 true로 설정된 경로와 함께 곧 호출됩니다.
		///
		/// 이 작업은 캐릭터의 이동을 중지하지 않고, 경로 계산만 중단합니다.
		/// </summary>
		/// <param name="pool">참이면 경로가 시스템에서 사용 완료될 때 경로가 풀에 들어갑니다.</param>
		public void CancelCurrentPathRequest(bool pool = true)
		{
			if (!IsDone()) {
				path.FailWithError("스크립트에 의해 취소됨 (Seeker.CancelCurrentPathRequest)");
				if (pool) {
					// 경로가 참조 카운트가 증가하고 감소되었음을 확인합니다.
					// 이렇게 하지 않으면 시스템은 풀링이 전혀 사용되지 않는 것으로 간주하고 경로를 풀에 저장하지 않습니다.
					// 이 매개 변수(이 경우 'path')를 사용하는 특정 개체는 전혀 중요하지 않습니다.
					// 그저 *어떤* 개체여야 합니다.
					path.Claim(path);
					path.Release(path);
				}
			}
		}

		/// <summary>
		/// 몇 가지 변수를 정리합니다.
		/// 필요한 경우 요청된 경로를 해제합니다.
		/// <see cref="startEndModifier"/>에서 OnDestroy를 호출합니다.
		///
		/// 참조: ReleaseClaimedPath
		/// 참조: startEndModifier
		/// </summary>
		public void OnDestroy () {
			ReleaseClaimedPath();
			startEndModifier.OnDestroy(this);
		}

		/// <summary>
		/// 경로 지도의 경로를 해제합니다 (있는 경우).
		/// Seeker는 최신 경로를 유지하여 Gizmos를 그릴 수 있습니다.
		/// 경우에 따라 이것은 원하지 않을 수 있으며, 해제하려면 이 메서드를 호출할 수 있습니다 (경로 Gizmos는 그려지지 않음에 유의).
		///
		/// 설명에서 아무것도 이해하지 못했다면 아마도 이 메서드를 사용할 필요가 없을 것입니다.
		///
		/// 참조: pooling (작동 링크가 있는 온라인 설명서에서 확인)
		/// </summary>
		void ReleaseClaimedPath () {
			if (prevPath != null) {
				prevPath.Release(this, true);
				prevPath = null;
			}
		}

		/// <summary>수정기가 자신을 등록할 때 호출됩니다.</summary>
		public void RegisterModifier (IPathModifier modifier) {
			modifiers.Add(modifier);

			// 명시된 순서를 기반으로 수정기를 정렬합니다.
			modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
		}

		/// <summary>수정기가 비활성화되거나 파괴될 때 수정기가 호출합니다.</summary>
		public void DeregisterModifier (IPathModifier modifier) {
			modifiers.Remove(modifier);
		}

		/// <summary>
		/// 경로를 후처리합니다.
		/// 이는 이 GameObject에 연결된 모든 수정기를 경로에 적용합니다.
		/// 이것은 RunModifiers(ModifierPass.PostProcess, path)를 호출하는 것과 동일합니다.
		/// 참조: RunModifiers
		/// \since 3.2에 추가됨
		/// </summary>
		public void PostProcess (Path path) {
			RunModifiers(ModifierPass.PostProcess, path);
		}

		/// <summary>경로에 수정기를 실행합니다.</summary>
		public void RunModifiers (ModifierPass pass, Path path) {
			if (pass == ModifierPass.PreProcess) {
				if (preProcessPath != null) preProcessPath(path);

				for (int i = 0; i < modifiers.Count; i++) modifiers[i].PreProcess(path);
			} else if (pass == ModifierPass.PostProcess) {
				Profiler.BeginSample("Running Path Modifiers");
				// Call delegates if they exist
				if (postProcessPath != null) postProcessPath(path);

				// Loop through all modifiers and apply post processing
				for (int i = 0; i < modifiers.Count; i++) modifiers[i].Apply(path);
				Profiler.EndSample();
			}
		}

		/// <summary>
		/// 현재 경로가 계산 완료되었는지 확인합니다.
		/// 현재 경로가 반환되었거나 경로가 null인 경우 true를 반환합니다.
		///
		/// 참고: 이것을 Pathfinding.Path.IsDone과 혼동하지 마십시오.
		/// 둘은 보통 같은 값을 반환하지만 항상 그런 것은 아니기 때문입니다.
		/// 경로가 완전히 계산되었지만 아직 Seeker에 의해 처리되지 않았을 수 있습니다.
		///
		/// \since 3.0.8에 추가됨
		/// 버전: 3.2에서 동작이 변경되었습니다
		/// </summary>
		public bool IsDone () {
			return path == null || path.PipelineState >= PathState.Returned;
		}

		/// <summary>
		/// 경로가 완료되었을 때 호출됩니다.
		/// 이 메서드는 선택적 매개변수 값으로 구현되어야 했지만 델리게이트와 잘 작동하지 않아보였습니다 (값은 기본값이 아니었습니다).
		/// 참조: OnPathComplete(Path,bool,bool)
		/// </summary>
		void OnPathComplete (Path path) {
			OnPathComplete(path, true, true);
		}

		/// <summary>
		/// 경로가 완료되었을 때 호출됩니다.
		/// 이 메서드는 <see cref="tmpPathCallback"/> 및 <see cref="pathCallback"/>을 호출하여 후처리하고 반환합니다.
		/// </summary>
		void OnPathComplete (Path p, bool runModifiers, bool sendCallbacks) {
			if (p != null && p != path && sendCallbacks) {
				return;
			}

			if (this == null || p == null || p != path)
				return;

			if (!path.error && runModifiers) {
				// 이것은 이 Seeker에 연결된 수정기에 경로를 후처리하기 위해 전송합니다
				RunModifiers(ModifierPass.PostProcess, path);
			}

			if (sendCallbacks) {
				p.Claim(this);

				lastCompletedNodePath = p.path;
				lastCompletedVectorPath = p.vectorPath;

				// 이것은 StartPath를 호출할 때 지정한 경우 경로에 대한 콜백에 경로를 보냅니다.
				if (tmpPathCallback != null) {
					tmpPathCallback(p);
				}

				// 이것은 콜백에 등록된 스크립트에 경로를 보냅니다.
				if (pathCallback != null) {
					pathCallback(p);
				}

				// 참고: gizmos를 그리려면 #prevPath가 유지되어야 합니다 (즉, 풀링되지 않음)
				// 현재 경로를 가져오는 메서드에서 반환될 수 있기 때문에 #path가 유지되어야 합니다.
				// #path가 #prevPath로 복사될 것이므로 #prevPath가 교체될 때까지 #prevPath를 유지하는 것만으로 충분합니다.

				// 이전 경로를 재활용하여 GC에 대한 부하를 줄입니다
				if (prevPath != null) {
					prevPath.Release(this, true);
				}

				prevPath = p;
			}
		}

		/// <summary>
		/// 이 함수를 호출하여 경로 계산을 시작합니다.
		/// 이 메서드는 콜백 매개변수를 사용하지 않으므로 이 메서드를 호출하기 전에 <see cref="pathCallback"/> 필드를 설정해야 합니다.
		/// </summary>
		/// <param name="start">경로의 시작점</param>
		/// <param name="end">경로의 끝점</param>
		public Path StartPath (Vector3 start, Vector3 end) {
			return StartPath(start, end, null);
		}

		/// <summary>
		/// 이 함수를 호출하여 경로 계산을 시작합니다.
		///
		/// 경로가 계산되면 콜백이 호출됩니다.
		/// 경로가 취소되지 않으면 (이전 경로가 완료되기 전에 새 경로가 요청되는 경우 등) 콜백이 호출되지 않습니다.
		/// </summary>
		/// <param name="start">경로의 시작점</param>
		/// <param name="end">경로의 끝점</param>
		/// <param name="callback">경로가 계산된 후 호출할 함수</param>
		public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback) {
			return StartPath(ABPath.Construct(start, end, null), callback);
		}

		/// <summary>
		/// 이 함수를 호출하여 경로 계산을 시작합니다.
		///
		/// 경로가 계산되면 콜백이 호출됩니다.
		/// 경로가 취소되지 않으면 (이전 경로가 완료되기 전에 새 경로가 요청되는 경우 등) 콜백이 호출되지 않습니다.
		/// </summary>
		/// <param name="start">경로의 시작점</param>
		/// <param name="end">경로의 끝점</param>
		/// <param name="callback">경로가 계산된 후 호출할 함수</param>
		/// <param name="graphMask">가까운 노드를 검색할 때 검색할 그래프를 지정하는 데 사용되는 마스크입니다. #Pathfinding.NNConstraint.graphMask를 참조하십시오. 이것은이 경로 요청의 #graphMask를 재정의합니다.</param>
		public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback, GraphMask graphMask)
		{
			return StartPath(ABPath.Construct(start, end, null), callback, graphMask);
		}

		/// <summary>
		/// 경로 계산을 시작하려면이 함수를 호출하십시오.
		///
		/// 경로가 계산되면 콜백이 호출됩니다 (미래 여러 프레임일 수 있음).
		/// 새 경로 요청이 계산되기 전에이 경로 요청이 계산되지 않으면 콜백이 호출되지 않습니다.
		///
		/// 버전: 3.8.3 이후로이 메서드는 MultiTargetPath가 사용될 경우 제대로 작동합니다.
		/// 이제 StartMultiTargetPath(MultiTargetPath) 메서드와 동일하게 작동합니다.
		///
		/// 버전: 4.1.x 이후로이 메서드는 경로 위의 graphMask를 매개변수로 전달하지 않는 한 경로 위의 graphMask를 덮어쓰지 않습니다. (이 메서드의 다른 오버로드 참조). </summary>
		/// <param name="p">계산을 시작할 경로</param>
		/// <param name="callback">경로가 계산된 후 호출할 함수</param>
		public Path StartPath(Path p, OnPathDelegate callback = null)
		{
			// 사용자가 기본값에서 그래프 마스크를 변경하지 않았다면 그래프 마스크를 설정합니다.
			// 이는 사용자가 정확히 -1로 설정하려고 했을 수 있기 때문에 완벽하지는 않지만, 제가 할 수 있는 최상의 감지입니다.
			// 기본값이 아닌 확인은 기존 코드를 깨트리지 않기 위한 호환성 이유로 주로 사용됩니다.
			// 그래프 마스크를 설정하려면이 메서드 대신 그래프 마스크 필드가 있는이 메서드의 다른 오버로드를 사용해야 합니다.
			if (p.nnConstraint.graphMask == -1) p.nnConstraint.graphMask = graphMask;
			StartPathInternal(p, callback);
			return p;
		}

		/// <summary>
		/// 경로 계산을 시작하려면이 함수를 호출하십시오.
		///
		/// 경로가 계산되면 콜백이 호출됩니다 (미래 여러 프레임일 수 있음).
		/// 새 경로 요청이 계산되기 전에이 경로 요청이 계산되지 않으면 콜백이 호출되지 않습니다.
		///
		/// 버전: 3.8.3 이후로이 메서드는 MultiTargetPath가 사용될 경우 제대로 작동합니다.
		/// 이제 StartMultiTargetPath(MultiTargetPath) 메서드와 동일하게 작동합니다.
		/// </summary>
		/// <param name="p">계산을 시작할 경로</param>
		/// <param name="callback">경로가 계산된 후 호출할 함수</param>
		/// <param name="graphMask">가까운 노드를 검색할 때 검색할 그래프를 지정하는 데 사용되는 마스크입니다. #Pathfinding.GraphMask를 참조하십시오. 이것은이 경로 요청의 #graphMask를 재정의합니다.</param>
		public Path StartPath(Path p, OnPathDelegate callback, GraphMask graphMask)
		{
			p.nnConstraint.graphMask = graphMask;
			StartPathInternal(p, callback);
			return p;
		}

		/// <summary>현재 경로를 시작하고 현재 활성 경로로 표시하는 내부 메서드입니다.</summary>
		void StartPathInternal(Path p, OnPathDelegate callback)
		{
			p.callback += onPathDelegate;

			p.enabledTags = traversableTags;
			p.tagPenalties = tagPenalties;

			// 경로가 처리 중이며 완료 상태가 오류가 아닌 경우 이전 경로가 아직 처리되지 않았기 때문에
			// 경로를 취소하고 또 다른 곳에서 재활용되지 않았는지 확인합니다.
			if (path != null && path.PipelineState <= PathState.Processing && path.CompleteState != PathCompleteState.Error && lastPathID == path.pathID)
			{
				path.FailWithError("새 경로가 요청될 때 경로가 취소되었습니다.\n" +
					"이것은 이미 계산 중인 경로가 이미 요청된 상태에서 새 경로가 계산되기를 기다리지 않고 직접 요청하기 위해 " +
					"Seeker에서 새 경로를 요청할 때 발생합니다. 아마도 당신이 원하는 것일 것입니다.\n" +
					"이를 자주 받는 경우 경로 요청을 어떻게 예약하는지 고려해 볼 필요가 있습니다.");
				// 취소된 경로에 대한 콜백은 보내지지 않습니다.
			}

			// 현재 경로를 활성 경로로 설정합니다.
			path = p;
			tmpPathCallback = callback;

			// 새 경로가 재활용되지 않도록 경로 ID를 저장합니다.
			lastPathID = path.pathID;

			// 경로를 사전 처리합니다.
			RunModifiers(ModifierPass.PreProcess, path);

			// 요청을 패스파인더로 전송합니다.
			AstarPath.StartPath(path);
		}


		/// <summary>Seeker의 gizmo를 그립니다.</summary>
		public void OnDrawGizmos () {
			if (lastCompletedNodePath == null || !drawGizmos) {
				return;
			}

			if (detailedGizmos) {
				Gizmos.color = new Color(0.7F, 0.5F, 0.1F, 0.5F);

				if (lastCompletedNodePath != null) {
					for (int i = 0; i < lastCompletedNodePath.Count-1; i++) {
						Gizmos.DrawLine((Vector3)lastCompletedNodePath[i].position, (Vector3)lastCompletedNodePath[i+1].position);
					}
				}
			}

			Gizmos.color = new Color(0, 1F, 0, 1F);

			if (lastCompletedVectorPath != null) {
				for (int i = 0; i < lastCompletedVectorPath.Count-1; i++) {
					Gizmos.DrawLine(lastCompletedVectorPath[i], lastCompletedVectorPath[i+1]);
				}
			}
		}

		protected override int OnUpgradeSerializedData (int version, bool unityThread) {
			if (graphMaskCompatibility != -1) {
				Debug.Log("Loaded " + graphMaskCompatibility + " " + graphMask.value);
				graphMask = graphMaskCompatibility;
				graphMaskCompatibility = -1;
			}
			return base.OnUpgradeSerializedData(version, unityThread);
		}
	}
}
