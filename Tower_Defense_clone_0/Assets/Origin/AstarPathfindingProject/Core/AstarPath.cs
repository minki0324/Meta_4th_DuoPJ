using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

#if NETFX_CORE
using Thread = Pathfinding.WindowsStore.Thread;
#else
using Thread = System.Threading.Thread;
#endif

[ExecuteInEditMode]
[AddComponentMenu("Pathfinding/Pathfinder")]
/// <summary>
/// A* 경로 찾기 시스템의 핵심 컴포넌트입니다.
/// 이 클래스는 모든 경로 찾기 시스템을 처리하고 모든 경로를 계산하며 정보를 저장합니다.
/// 이 클래스는 싱글톤 클래스로, 장면에서 최대 하나의 활성 인스턴스만 있어야 합니다.
/// 일반적으로 직접 사용하기는 어렵고 주로 <see cref="Pathfinding.Seeker"/> 클래스를 통해 경로 찾기 시스템을 사용합니다.
///
/// \nosubgrouping
/// \ingroup relevant
/// </summary>
[HelpURL("http://arongranberg.com/astar/docs/class_astar_path.php")]
public class AstarPath : VersionedMonoBehaviour {
	/// <summary>A* %Pathfinding Project의 버전 번호입니다.</summary>
	public static readonly System.Version Version = new System.Version(4, 2, 17);

	/// <summary>패키지 다운로드 위치에 대한 정보입니다.</summary>
	public enum AstarDistribution { WebsiteDownload, AssetStore, PackageManager };

	/// <summary>사용자를 올바른 업데이트 다운로드 위치로 안내하기 위해 에디터에서 사용됩니다.</summary>
	public static readonly AstarDistribution Distribution = AstarDistribution.WebsiteDownload;

	/// <summary>
	/// 이 릴리스의 A* %Pathfinding Project 브랜치입니다.
	/// 업데이트를 확인하여 개발 버전의 사용자가 개발 업데이트 알림을 받을 수 있도록 사용됩니다.
	/// </summary>
	public static readonly string Branch = "master";

	/// <summary>모든 그래프 데이터를 보유합니다.</summary>
	[UnityEngine.Serialization.FormerlySerializedAs("astarData")]
	public AstarData data;

	/// <summary>
	/// 장면에서 활성 AstarPath 개체를 반환합니다.
	/// 참고: AstarPath 개체가 초기화되어야만 이 값이 설정됩니다 (이는 Awake에서 발생합니다).
	/// </summary>
#if UNITY_4_6 || UNITY_4_3
	public static new AstarPath active;
#else
	public static AstarPath active;
#endif

	/// <summary>Pathfinding.AstarData.graphs에 대한 단축 경로입니다.</summary>
	public NavGraph[] graphs {
		get {
			if (data == null)
				data = new AstarData();
			return data.graphs;
		}
	}

	#region InspectorDebug
	/// <summary>
	/// @name Inspector - 디버그
	/// @{
	/// </summary>

	/// <summary>
	/// 시각적으로 그래프를 씬 뷰에 표시합니다 (에디터 전용).
	/// </summary>
	public bool showNavGraphs = true;

	/// <summary>
	/// 산책할 수 없는 노드를 표시/숨깁니다.
	///
	/// 참고: 에디터에서만 관련됩니다.
	///
	/// 참조: <see cref="unwalkableNodeDebugSize"/>
	/// </summary>
	public bool showUnwalkableNodes = true;

	/// <summary>
	/// 씬 뷰에서 노드를 그릴 때 사용할 모드입니다.
	///
	/// 참고: 에디터에서만 관련됩니다.
	///
	/// 참조: Pathfinding.GraphDebugMode
	/// </summary>
	public GraphDebugMode debugMode;

	/// <summary>
	/// 일부 <see cref="debugMode"/> 모드에 사용할 낮은 값입니다.
	/// 예를 들어, <see cref="debugMode"/>가 G로 설정된 경우, 이 값은 노드가 완전히 빨간색이 될 때를 결정합니다.
	///
	/// 참고: 에디터에서만 관련됩니다.
	///
	/// 참조: <see cref="debugRoof"/>
	/// 참조: <see cref="debugMode"/>
	/// </summary>
	public float debugFloor = 0;

	/// <summary>
	/// 일부 <see cref="debugMode"/> 모드에 사용할 높은 값입니다.
	/// 예를 들어, <see cref="debugMode"/>가 G로 설정된 경우, 이 값은 노드가 완전히 녹색이 될 때를 결정합니다.
	///
	/// 패널티 디버그 모드의 경우, 패널티가 <see cref="debugFloor"/>보다 작을 때 노드는 녹색으로 색상이 지정되고,
	/// 패널티가 이 값보다 크거나 같을 때는 빨간색으로 지정되며 빨간색과 녹색 사이의 값으로 지정됩니다.
	///
	/// 참고: 에디터에서만 관련됩니다.
	///
	/// 참조: <see cref="debugFloor"/>
	/// 참조: <see cref="debugMode"/>
	/// </summary>
	public float debugRoof = 20000;

	/// <summary>
	/// 설정되면 <see cref="debugFloor"/> 및 <see cref="debugRoof"/> 값이 자동으로 다시 계산되지 않습니다.
	///
	/// 참고: 편집기에서만 관련됩니다.
	/// </summary>
	public bool manualDebugFloorRoof = false;


	/// <summary>
	/// 만약 활성화되면, 노드는 '부모'에 대한 선을 그립니다.
	/// 이것은 가장 최근 경로를 디버그하기 위해 노드를 사용하여 검색 트리를 보여줍니다.
	///
	/// 참고: 에디터에서만 관련됩니다.
	///
	/// TODO: showOnlyLastPath 플래그를 추가하여 모든 노드를 그리는 것 대신 가장 최근 경로에서 방문한 노드만 그릴지 여부를 지정합니다.
	/// </summary>
	public bool showSearchTree = false;

	/// <summary>
	/// 산책할 수 없는 노드 대신 빨간색 큐브의 크기입니다.
	///
	/// 참고: 에디터에서만 관련됩니다. 그리드 그래프에는 적용되지 않습니다.
	/// 참조: <see cref="showUnwalkableNodes"/>
	/// </summary>
	public float unwalkableNodeDebugSize = 0.3F;

	/// <summary>
	/// 디버깅 메시지의 양입니다.
	/// 성능을 개선하려면 (약간) 덜 디버깅하거나 콘솔 스팸을 제거하려면 덜 디버깅합니다.
	/// 세부 정보를 얻거나 경로 찾기 스크립트가 수행하는 작업에 대한 더 많은 정보를 원할 경우 (무거운) 더 많은 디버깅을 사용합니다.
	/// InGame 옵션은 최신 경로 로그를 게임 내 GUI를 사용하여 표시합니다.
	///
	/// [온라인 문서에서 이미지를 보려면 여기를 클릭하세요]
	/// </summary>
	public PathLog logPathResults = PathLog.Normal;

	/// <summary>@}</summary>
	#endregion

	#region InspectorSettings
	/// <summary>
	/// @name Inspector - 설정
	/// @{
	/// </summary>

	/// <summary>
	/// 노드를 검색하는 최대 거리입니다.
	/// 지점에 가장 가까운 노드를 검색할 때 이는 (세계 단위로) 허용되는 최대 거리입니다.
	///
	/// 이것은 접근할 수 없는 지점으로 경로를 요청하는 경우 해당 지점에 도달할 수 있는 가장 가까운 노드를 검색해야 하는 경우 관련됩니다.
	/// 이 거리 내에서 노드를 찾을 수 없으면 경로 요청이 실패합니다.
	///
	/// 참고: <see cref="Pathfinding.NNConstraint.constrainDistance"/>
	/// </summary>
	public float maxNearestNodeDistance = 100;

	/// <summary>
	/// 최대 인접 노드 거리의 제곱입니다.
	/// 참조: <see cref="maxNearestNodeDistance"/>
	/// </summary>
	public float maxNearestNodeDistanceSqr {
		get { return maxNearestNodeDistance*maxNearestNodeDistance; }
	}

	/// <summary>
	/// 시작할 때 모든 그래프를 스캔해야 하는지 여부입니다.
	/// 이것은 캐시에서 로드하는 것을 제외하고 모든 그래프를 스캔해야 하는지 여부를 결정합니다.
	/// 이 값을 비활성화하면 <see cref="Scan"/>을 직접 호출하거나 파일에서 저장된 그래프를 로드해야 합니다.
	///
	/// 참조: <see cref="Scan"/>
	/// 참조: <see cref="ScanAsync"/>
	/// </summary>
	public bool scanOnStartup = true;

	/// <summary>
	/// 모든 그래프에 대한 완전한 GetNearest 검색을 수행해야 하는지 여부입니다.
	/// 일반적으로 추가 검색은 첫 번째 빠른 검색에서 가장 가까운 노드를 찾은 그래프에서만 수행됩니다.
	/// 이 설정을 켜면 추가 검색이 모든 그래프에서 수행됩니다.
	///
	/// 보통 비활성화될 때 더 빠르지만 품질이 더 높은 검색이 가능합니다.
	/// 1개 또는 2개 이상의 코어를 가진 컴퓨터에서 더 빠른 스루풋을 원하지 않는 한, 대부분의 경우 비활성화하는 것이 좋습니다.
	/// 여러 코어를 가진 컴퓨터에서는 더 빠른 스루풋을 얻을 수 있으며 경로가 동일한 길이의 여러 개를 따라가지 않습니다.
	///
	/// 주의: 그리드 그래프에 사용할 경우 이 설정은 크게 중요하지 않습니다. 그래프에 대한 단일 검색 모드만 사용합니다.
	/// </summary>
	public bool fullGetNearestSearch = false;

	/// <summary>
	/// 그래프를 우선 순위에 따라 정렬합니다.
	/// 그래프는 인스펙터에서의 순서를 기준으로 우선 순위가 지정됩니다.
	/// <see cref="prioritizeGraphsLimit"/>보다 가까운 노드를 가진 그래프 중 첫 번째 그래프가 모든 그래프를 검색하는 대신 선택됩니다.
	/// </summary>
	public bool prioritizeGraphs = false;

	/// <summary>
	/// <see cref="prioritizeGraphs"/>에 대한 거리 제한입니다.
	/// 참고: <see cref="prioritizeGraphs"/>
	/// </summary>
	public float prioritizeGraphsLimit = 1F;

	/// <summary>
	/// 이 AstarPath 객체의 색상 설정에 대한 참조입니다.
	/// 색상 설정에는 예를 들어 씬 뷰에서 노드의 색상이어야 하는지 여부 등이 포함됩니다.
	/// </summary>
	public AstarColor colorSettings;

	/// <summary>
	/// 저장된 태그 이름입니다.
	/// 참조: AstarPath.FindTagNames
	/// 참조: AstarPath.GetTagNames
	/// </summary>
	[SerializeField]
	protected string[] tagNames = null;

	/// <summary>
	/// 휴리스틱(Heuristic)으로 사용할 거리 함수입니다.
	/// 휴리스틱은 노드에서 목표 지점까지의 예상 비용을 나타냅니다.
	/// 다른 휴리스틱은 동일한 길이의 다른 경로 중에서 어떤 경로를 선택할지에 영향을 미칩니다.
	/// 다양한 휴리스틱에 대한 자세한 내용 및 설명은 <see cref="Pathfinding.Heuristic"/>을 참조하세요.
	/// </summary>
	public Heuristic heuristic = Heuristic.Euclidean;

	/// <summary>
	/// 휴리스틱의 스케일입니다.
	/// 1보다 작은 값은 패스파인더가 더 많은 노드를 검색하게 하므로 느려집니다.
	/// 0을 사용하면 패스파인딩 알고리즘이 Dijkstra 알고리즘으로 감소됩니다. 이것은 <see cref="heuristic"/>을 None으로 설정한 것과 동일합니다.
	/// 1보다 큰 값을 사용하면 패스파인딩이 (일반적으로) 더 빨라지지만 경로가 최적(즉, 가능한 최단 경로)이 아닐 수 있습니다.
	///
	/// 보통 이 값을 1로 두는 것이 좋습니다.
	///
	/// 참조: https://en.wikipedia.org/wiki/Admissible_heuristic
	/// </summary>
	public float heuristicScale = 1F;

	/// <summary>
	/// 사용할 패스파인딩 스레드 수입니다.
	/// 멀티스레딩은 패스파인딩을 다른 스레드로 이동하여 2개 이상의 코어 컴퓨터에서 성능에 거의 영향을 주지 않고 프레임률을 유지할 수 있게 합니다.
	/// - None은 패스파인딩을 Unity 스레드에서 코루틴으로 실행함을 의미합니다.
	/// - Automatic은 스레드 수를 컴퓨터의 코어 수와 메모리에 맞게 조정하려고 시도합니다.
	///   512MB 미만의 메모리 또는 단일 코어 컴퓨터의 경우 멀티스레딩을 사용하지 않도록 되돌립니다.
	///
	/// 가능한 경우 "Auto" 설정 중 하나를 사용하는 것이 좋습니다.
	/// 이유는 컴퓨터가 강력하고 8개의 코어가 있는 경우에도
	/// 다른 컴퓨터는 쿼드 코어 또는 듀얼 코어이므로 1 또는 3개의 스레드 이상 사용하지 않습니다 (일반적으로 Unity 스레드에 하나를 남겨 두려고 함).
	/// 스레드 수를 컴퓨터의 코어 수보다 많이 사용하면 주로 메모리를 낭비하게 되어 더 빠르게 실행되지 않습니다.
	/// 추가 메모리 사용량은 무시할 수 없을 정도로 적지 않습니다. 각 스레드는 모든 그래프의 모든 노드에 대한 작은 양의 데이터를 유지해야 합니다.
	/// 전체 그래프 데이터가 아니지만 노드 수에 비례합니다.
	/// 자동 설정은 실행 중인 기기를 조사하고 메모리를 낭비하지 않도록 스레드 수를 결정합니다.
	///
	/// 예외는 한 번에 하나 또는 두 개의 캐릭터만 활성화하는 경우입니다. 그런 다음 일반적으로 스레드가 하나일 때 충분한데,
	/// 더 많은 스레드가 제공하는 추가 처리량이 필요할 가능성은 거의 없습니다. 더 많은 스레드가 제공하는 주요 이점은 다른 스레드에서 다른 경로를 계산하기 때문입니다.
	/// 개별 경로 계산은 스레드 수가 늘어나도 동일한 속도로 계산되지 않습니다.
	///
	/// 참고: 스레딩을 사용자 지정하는 경우 또는 안전한 래퍼를 사용하지 않고 그래프 데이터를 직접 수정하는 경우,
	/// 멀티스레딩은 이상한 오류를 유발하고 주의하지 않으면 패스파인딩이 작동을 멈출 수 있습니다.
	/// 기본 사용법(패스파인딩 코어 수정하지 않음)에 대해서는 안전합니다.
	///
	/// 참고: WebGL은 스레드를 전혀 지원하지 않으므로 해당 플랫폼에서는 스레드를 사용하지 않습니다.
	///
	/// 참조: CalculateThreadCount
	/// </summary>
	public ThreadCount threadCount = ThreadCount.One;

	/// <summary>
	/// 각 프레임에서 패스파인딩에 소요될 수 있는 최대 시간(밀리초)입니다.
	/// 최소 500개의 노드가 각 프레임에서 검색됩니다 (만약 검색할 노드가 그만큼 있다면).
	/// 멀티스레딩을 사용하는 경우 이 값은 무시됩니다.
	/// </summary>
	public float maxFrameTime = 1F;

	/// <summary>
	/// 성능을 향상시키기 위해 그래프 업데이트를 제한하고 배치합니다.
	/// 켜면 그래프 업데이트가 배치되어 덜 자주 실행됩니다 (그래프 업데이트 간격은 <see cref="graphUpdateBatchingInterval)"/>로 지정됨).
	///
	/// 패스파인딩 스레드를 자주 중지할 필요가 없으므로 패스파인딩 처리량에 긍정적인 영향을 미칠 수 있으며,
	/// 그래프 업데이트당 오버헤드를 줄일 수 있습니다.
	/// 모든 그래프 업데이트는 적용되지만, 모두 함께 배치되어 더 많이 적용됩니다.
	///
	/// 그러나 최소한의 대기 시간을 원하는 경우 사용하지 마세요.
	///
	/// 이것은 <see cref="UpdateGraphs"/> 메서드를 사용하여 요청된 그래프 업데이트에만 적용됩니다. <see cref="RegisterSafeUpdate"/> 또는 <see cref="AddWorkItem"/>을 사용하여 요청된 그래프 업데이트에는 적용되지 않습니다.
	///
	/// 언제든지 그래프 업데이트를 즉시 적용하려면 <see cref="FlushGraphUpdates"/>를 호출할 수 있습니다.
	///
	/// 참조: 그래프 업데이트 (온라인 문서에서 작동 링크 보기)
	/// </summary>
	public bool batchGraphUpdates = false;

	/// <summary>
	/// 각 그래프 업데이트 배치 간의 최소 초 단위 시간입니다.
	/// <see cref="batchGraphUpdates"/>가 true로 설정된 경우, 이것은 각 그래프 업데이트 배치 간의 최소 초 단위 시간을 정의합니다.
	///
	/// 패스파인딩 스레드를 자주 중지할 필요가 없으므로 패스파인딩 처리량에 긍정적인 영향을 미칠 수 있으며,
	/// 그래프 업데이트당 오버헤드를 줄일 수 있습니다.
	/// 모든 그래프 업데이트는 적용되지만, 모두 함께 배치되어 더 많이 적용됩니다.
	///
	/// 최소한의 대기 시간을 원하는 경우 사용하지 마세요.
	///
	/// 이것은 <see cref="UpdateGraphs"/> 메서드를 사용하여 요청된 그래프 업데이트에만 적용됩니다. <see cref="RegisterSafeUpdate"/> 또는 <see cref="AddWorkItem"/>을 사용하여 요청된 그래프 업데이트에는 적용되지 않습니다.
	///
	/// 참조: 그래프 업데이트 (온라인 문서에서 작동 링크 보기)
	/// </summary>
	public float graphUpdateBatchingInterval = 0.2F;

	/// <summary>
	/// 그래프 업데이트 배치를 제한합니다.
	/// Deprecated: 이 필드는 'batchGraphUpdates'로 이름이 변경되었습니다.
	/// </summary>
	[System.Obsolete("This field has been renamed to 'batchGraphUpdates'")]
	public bool limitGraphUpdates { get { return batchGraphUpdates; } set { batchGraphUpdates = value; } }

	/// <summary>
	/// 그래프 업데이트 최대 빈도에 대한 제한입니다.
	/// Deprecated: 이 필드는 'graphUpdateBatchingInterval'로 이름이 변경되었습니다.
	/// </summary>
	[System.Obsolete("This field has been renamed to 'graphUpdateBatchingInterval'")]
	public float maxGraphUpdateFreq { get { return graphUpdateBatchingInterval; } set { graphUpdateBatchingInterval = value; } }

	/// <summary>@}</summary>
	#endregion

	#region DebugVariables
	/// <summary>
	/// @name 디버그 멤버
	/// @{
	/// </summary>

#if ProfileAstar
/// <summary>
/// 애플리케이션 시작부터 실행된 경로 수입니다.\n
/// 디버깅 변수
/// </summary>
	public static int PathsCompleted = 0;

	public static System.Int64 TotalSearchedNodes = 0;
	public static System.Int64 TotalSearchTime = 0;
#endif

	/// <summary>
	/// 마지막으로 Scan() 호출이 완료된 데 걸린 시간입니다.
	/// 그래프를 너무 자주 자동으로 다시 스캔하는 것을 방지하는 데 사용됩니다 (에디터 전용)
	/// </summary>
	public float lastScanTime { get; private set; }

	/// <summary>
	/// 지점을 디버깅하기 위해 사용되는 경로입니다.
	/// 이 경로 핸들러는 마지막 경로를 계산하는 데 사용됩니다.
	/// 에디터에서 gizmo를 사용하여 디버그 정보를 그릴 때 사용됩니다.
	/// </summary>
	[System.NonSerialized]
	public PathHandler debugPathData;

	/// <summary>gizmo를 사용하여 디버깅할 경로 ID입니다</summary>
	[System.NonSerialized]
	public ushort debugPathID;

	/// <summary>
	/// 마지막 완료된 경로에서 디버그 문자열입니다.
	/// <see cref="logPathResults"/> == PathLog.InGame인 경우 업데이트됩니다.
	/// </summary>
	string inGameDebugPath;

	/* @} */
	#endregion

	#region StatusVariables

	/// <summary>
	/// <see cref="isScanning"/>에 대한 백업 필드입니다.
	/// System.NonSerialized로 표시할 수 없기 때문에 자동 속성을 사용할 수 없습니다.
	/// </summary>
	[System.NonSerialized]
	bool isScanningBacking;

	/// <summary>
	/// 그래프가 스캔 중일 때 설정됩니다.
	/// FloodFill이 완료될 때까지 true가 됩니다.
	///
	/// 참고: 그래프 업데이트와 혼동하지 마십시오.
	///
	/// OnPostScan에서 호출되는 Graph Update Object를 더 잘 지원하기 위해 사용됩니다.
	///
	/// 참조: IsAnyGraphUpdateQueued
	/// 참조: IsAnyGraphUpdateInProgress
	/// </summary>
	public bool isScanning { get { return isScanningBacking; } private set { isScanningBacking = value; } }

	/// <summary>
	/// 병렬 패스파인더 수입니다.
	/// 한 번에 경로를 계산할 수 있는 동시 프로세스 수를 반환합니다.
	/// 멀티스레딩을 사용하는 경우 이는 스레드 수일 것이며, 멀티스레딩을 사용하지 않는 경우 항상 1입니다 (코루틴만 사용하기 때문).
	/// 참조: IsUsingMultithreading
	/// </summary>
	public int NumParallelThreads {
		get {
			return pathProcessor.NumThreads;
		}
	}

	/// <summary>
	/// 멀티스레딩을 사용하는지 여부를 반환합니다.
	/// \exception System.Exception 멀티스레딩을 사용하는지 여부를 결정할 수 없을 때 throw됩니다.
	/// 이는 경로 찾기가 올바르게 설정되지 않았을 때 발생하지 않아야 합니다.
	/// 참고: 이는 현재 스레드가 실행 중인지 여부에 대한 정보를 사용합니다. A* 객체의 설정에서의 정보는 사용하지 않습니다.
	/// </summary>
	public bool IsUsingMultithreading {
		get {
			return pathProcessor.IsUsingMultithreading;
		}
	}

	/// <summary>
	/// 대기 중인 그래프 업데이트가 있는지 여부를 반환합니다.
	/// <see cref="IsAnyGraphUpdateQueued"/>를 대신 사용하세요.
	/// </summary>
	[System.Obsolete("Fixed grammar, use IsAnyGraphUpdateQueued instead")]
	public bool IsAnyGraphUpdatesQueued { get { return IsAnyGraphUpdateQueued; } }

	/// <summary>
	/// 대기 중인 그래프 업데이트가 있는지 여부를 반환합니다.
	/// 참고: 업데이트가 수행 중인 동안 false입니다.
	/// 참고: 이것은 그래프 업데이트에만 해당됩니다. <see cref="RegisterSafeUpdate"/> 또는 <see cref="AddWorkItem"/>로 추가된 다른 유형의 작업 항목은 포함하지 않습니다.
	/// </summary>
	public bool IsAnyGraphUpdateQueued { get { return graphUpdates.IsAnyGraphUpdateQueued; } }

	/// <summary>
	/// 현재 그래프 업데이트가 진행 중인지 여부를 반환합니다.
	/// 참고: 이것은 그래프 업데이트에만 해당됩니다. <see cref="RegisterSafeUpdate"/> 또는 <see cref="AddWorkItem"/>로 추가된 다른 유형의 작업 항목은 포함하지 않습니다.
	///
	/// 참조: IsAnyWorkItemInProgress
	/// </summary>
	public bool IsAnyGraphUpdateInProgress { get { return graphUpdates.IsAnyGraphUpdateInProgress; } }

	/// <summary>
	/// 현재 작업 항목이 진행 중인지 여부를 반환합니다.
	/// 참고: 이것은 대부분의 그래프 업데이트 유형을 포함합니다.
	/// 일반 그래프 업데이트, 네비메시 자르기 및 <see cref="RegisterSafeUpdate"/> 또는 <see cref="AddWorkItem"/>로 추가된 모든 작업 항목과 같습니다.
	/// </summary>
	public bool IsAnyWorkItemInProgress { get { return workItems.workItemsInProgress; } }

	/// <summary>
	/// 현재 이 코드가 작업 항목 내에서 실행되고 있는지 여부를 반환합니다.
	/// 참고: 이것은 대부분의 그래프 업데이트 유형을 포함합니다.
	/// 일반 그래프 업데이트, 네비메시 자르기 및 <see cref="RegisterSafeUpdate"/> 또는 <see cref="AddWorkItem"/>로 추가된 모든 작업 항목과 같습니다.
	///
	/// <see cref="IsAnyWorkItemInProgress"/>와 달리 이 값은 작업 항목 코드가 실행 중일 때만 true이며, 여러 프레임에 걸쳐 작업 항목을 업데이트하는 동안 true가 아닙니다.
	/// </summary>
	internal bool IsInsideWorkItem { get { return workItems.workItemsInProgressRightNow; } }

	#endregion

	#region Callbacks
	/// <summary>@name 콜백</summary>
	/* 경로 찾기 이벤트에 대한 콜백입니다.
	 * 이를 통해 경로 찾기 프로세스에 훅할 수 있습니다.
	 * 콜백은 다음과 같이 사용할 수 있습니다:
	 * \snippet MiscSnippets.cs AstarPath.Callbacks
	 */
	/// <summary>@{</summary>

	/// <summary>
	/// Awake 이후에 모든 작업을 시작하기 전에 호출됩니다.
	/// 이 Awake 호출의 시작에서 호출되며 <see cref="active"/>가 설정된 직후이지만 이것 외에는 아무것도 수행되지 않았습니다.
	/// 런타임 중에 생성된 AstarPath 구성 요소에 대한 기본 설정을 설정하려면이 Awake에서만 변경할 수있는 몇 가지 설정을 사용하십시오
	/// (멀티스레딩 관련 설정 등)
	/// <code>
	/// // 시작 시 새 AstarPath 개체를 만들고 기본 설정을 적용합니다
	/// public void Start () {
	///     AstarPath.OnAwakeSettings += ApplySettings;
	///     AstarPath astar = gameObject.AddComponent<AstarPath>();
	/// }
	///
	/// public void ApplySettings () {
	///     // 대리자에서 등록 해제
	///     AstarPath.OnAwakeSettings -= ApplySettings;
	///     // 예를 들어 Awake 호출 이후에 threadCount를 변경할 수 없으므로 여기에서만 설정할 수 있습니다.
	///     AstarPath.active.threadCount = ThreadCount.One;
	/// }
	/// </code>
	/// </summary>
	public static System.Action OnAwakeSettings;

	/// <summary>그래프가 스캔되기 전에 각 그래프에 대해 호출됩니다.</summary>
	public static OnGraphDelegate OnGraphPreScan;

	/// <summary>그래프가 스캔된 후에 각 그래프에 대해 호출됩니다. 다른 그래프는 아직 스캔되지 않았을 수 있습니다.</summary>
	public static OnGraphDelegate OnGraphPostScan;

	/// <summary>검색하기 전에 각 경로에 대해 호출됩니다. 멀티스레딩을 사용할 때 주의하십시오. 이것은 다른 스레드에서 호출됩니다.</summary>
	public static OnPathDelegate OnPathPreSearch;

	/// <summary>검색 후 각 경로에 대해 호출됩니다. 멀티스레딩을 사용할 때 주의하십시오. 이것은 다른 스레드에서 호출됩니다.</summary>
	public static OnPathDelegate OnPathPostSearch;

	/// <summary>스캔 시작 전에 호출됩니다.</summary>
	public static OnScanDelegate OnPreScan;

	/// <summary>스캔 후에 호출됩니다. 이것은 링크를 적용하고 그래프를 침수 채우기 및 기타 후 처리를 수행하기 전에 호출됩니다.</summary>
	public static OnScanDelegate OnPostScan;

	/// <summary>스캔이 완전히 완료되면 호출됩니다. 이것은 Scan 함수에서 마지막으로 호출되는 것으로 호출됩니다.</summary>
	public static OnScanDelegate OnLatePostScan;

	/// <summary>그래프가 업데이트 될 때 호출됩니다. 예를 들어 그래프가 변경될 때마다 경로를 다시 계산하도록 등록합니다.</summary>
	public static OnScanDelegate OnGraphsUpdated;

	/// <summary>
	/// pathID가 65536을 넘어서면 0으로 재설정될 때 호출됩니다.
	/// 참고: 이 콜백은 호출될 때마다 지워지므로 다시 등록하려면 바로 콜백을 받은 후에 직접 등록하십시오.
	/// </summary>
	public static System.Action On65KOverflow;

	/// <summary>사용하지 않음:</summary>
	[System.ObsoleteAttribute]
	public System.Action OnGraphsWillBeUpdated;

	/// <summary>사용하지 않음:</summary>
	[System.ObsoleteAttribute]
	public System.Action OnGraphsWillBeUpdated2;

	/* @} */
	#endregion

	#region MemoryStructures

	/// <summary>그래프 업데이트를 처리합니다.</summary>
	readonly GraphUpdateProcessor graphUpdates;

	/// <summary>두 노드 사이에 경로가 있는지와 같은 일부 쿼리를 가속화하기 위해 계층 그래프를 보관합니다.</summary>
	internal readonly HierarchicalGraph hierarchicalGraph = new HierarchicalGraph();

	/// <summary>
	/// 네비메시 컷을 처리합니다.
	/// 참조: <see cref="Pathfinding.NavmeshCut"/>
	/// </summary>
	public readonly NavmeshUpdates navmeshUpdates = new NavmeshUpdates();

	/// <summary>작업 항목을 처리합니다.</summary>
	readonly WorkItemProcessor workItems;

	/// <summary>대기 중인 모든 경로를 보관하고 계산합니다.</summary>
	PathProcessor pathProcessor;

	bool graphUpdateRoutineRunning = false;

	/// <summary>QueueGraphUpdates가 여러 번 그래프 업데이트 명령을 대기열에 추가하지 않도록 합니다.</summary>
	bool graphUpdatesWorkItemAdded = false;

	/// <summary>
	/// 마지막 그래프 업데이트가 완료된 시간입니다.
	/// 자주 그래프 업데이트를 묶어 배치로 그룹화하기 위해 사용됩니다.
	/// </summary>
	float lastGraphUpdate = -9999F;

	/// <summary>현재 대기 중인 작업 항목이 있는 경우 보관합니다.</summary>
	PathProcessor.GraphUpdateLock workItemLock;

	/// <summary>보관된 모든 완료된 경로를 반환하도록 대기합니다.</summary>
	internal readonly PathReturnQueue pathReturnQueue;

	/// <summary>
	/// 휴리스틱 최적화를위한 설정을 보관합니다.
	/// 참조: heuristic-opt (작동 링크는 온라인 설명서에서 확인하십시오)
	/// </summary>
	public EuclideanEmbedding euclideanEmbedding = new EuclideanEmbedding();

	#endregion

	/// <summary>
	/// 그래프 검사기를 표시하거나 숨깁니다.
	/// 내부에서 에디터에 의해 사용됩니다.
	/// </summary>
	public bool showGraphs = false;

	/// <summary>
	/// 그래프 검사기를 표시하거나 숨깁니다.
	/// 내부에서 에디터에 의해 사용됩니다.
	/// </summary>
	private ushort nextFreePathID = 1;

	private AstarPath () {
		pathReturnQueue = new PathReturnQueue(this);

		// pathProcessor가 null이 되지 않도록 합니다.
		pathProcessor = new PathProcessor(this, pathReturnQueue, 1, false);

		workItems = new WorkItemProcessor(this);
		graphUpdates = new GraphUpdateProcessor(this);

		// graphUpdates.OnGraphsUpdated를 AstarPath.OnGraphsUpdated로 전달합니다.
		graphUpdates.OnGraphsUpdated += () => {
			if (OnGraphsUpdated != null) {
				OnGraphsUpdated(this);
			}
		};
	}

	/// <summary>
	/// 태그 이름을 반환합니다.
	/// 태그 이름 배열이 null이거나 길이가 32가 아닌 경우 새 배열을 만들고 0,1,2,3,4 등으로 채웁니다.
	/// </summary>
	public string[] GetTagNames () {
		if (tagNames == null || tagNames.Length != 32) {
			tagNames = new string[32];
			for (int i = 0; i < tagNames.Length; i++) {
				tagNames[i] = ""+i;
			}
			tagNames[0] = "Basic Ground";
		}
		return tagNames;
	}

	/// <summary>
	/// 실행 모드 외부에서 AstarPath 개체를 선택되지 않았더라도 초기화합니다.
	/// 이렇게하면 <see cref="active"/> 속성이 설정되고 모든 그래프가 역직렬화됩니다.
	///
	/// 이것은 실행 모드에서 그래프를 편집하려고하지만 그래프가 아직 역직렬화되지 않았을 때 유용합니다.
	/// 실행 모드에서이 메서드는 아무 작업도 수행하지 않습니다.
	/// </summary>
	public static void FindAstarPath () {
		if (Application.isPlaying) return;
		if (active == null) active = GameObject.FindObjectOfType<AstarPath>();
		if (active != null && (active.data.graphs == null || active.data.graphs.Length == 0)) active.data.DeserializeGraphs();
	}

	/// <summary>
	/// AstarPath 개체를 찾고 태그 이름을 반환하려고 시도합니다.
	/// AstarPath 개체를 찾을 수 없으면 길이가 1인 오류 메시지를 포함하는 배열을 반환합니다.
	/// </summary>
	public static string[] FindTagNames () {
		FindAstarPath();
		return active != null? active.GetTagNames () : new string[1] { "There is no AstarPath component in the scene" };
	}

	/// <summary>다음 무료 경로 ID를 반환합니다</summary>
	internal ushort GetNextPathID () {
		if (nextFreePathID == 0) {
			nextFreePathID++;

			if (On65KOverflow != null) {
				System.Action tmp = On65KOverflow;
				On65KOverflow = null;
				tmp();
			}
		}
		return nextFreePathID++;
	}

	void RecalculateDebugLimits () {
		debugFloor = float.PositiveInfinity;
		debugRoof = float.NegativeInfinity;

		bool ignoreSearchTree = !showSearchTree || debugPathData == null;
		for (int i = 0; i < graphs.Length; i++) {
			if (graphs[i] != null && graphs[i].drawGizmos) {
				graphs[i].GetNodes(node => {
					if (node.Walkable && (ignoreSearchTree || Pathfinding.Util.GraphGizmoHelper.InSearchTree(node, debugPathData, debugPathID))) {
						if (debugMode == GraphDebugMode.Penalty) {
							debugFloor = Mathf.Min(debugFloor, node.Penalty);
							debugRoof = Mathf.Max(debugRoof, node.Penalty);
						} else if (debugPathData != null) {
							var rnode = debugPathData.GetPathNode(node);
							switch (debugMode) {
							case GraphDebugMode.F:
								debugFloor = Mathf.Min(debugFloor, rnode.F);
								debugRoof = Mathf.Max(debugRoof, rnode.F);
								break;
							case GraphDebugMode.G:
								debugFloor = Mathf.Min(debugFloor, rnode.G);
								debugRoof = Mathf.Max(debugRoof, rnode.G);
								break;
							case GraphDebugMode.H:
								debugFloor = Mathf.Min(debugFloor, rnode.H);
								debugRoof = Mathf.Max(debugRoof, rnode.H);
								break;
							}
						}
					}
				});
			}
		}

		if (float.IsInfinity(debugFloor)) {
			debugFloor = 0;
			debugRoof = 1;
		}

		// 두 값이 동일하지 않도록하십시오. 이렇게하면 색상 보간이 실패합니다.
		if (debugRoof-debugFloor < 1) debugRoof += 1;
	}

	Pathfinding.Util.RetainedGizmos gizmos = new Pathfinding.Util.RetainedGizmos();

	/// <summary>
	/// 그래프 생성기에서 OnDrawGizmos를 호출합니다.
	/// </summary>
	private void OnDrawGizmos () {
		// 싱글톤 패턴이 유지되도록 합니다.
		// Awake 메서드가 호출되지 않은 경우에만 해당할 수 있습니다.
		if (active == null) active = this;

		if (active != this || graphs == null) {
			return;
		}

		// Unity에서는 마우스로 개체를 클릭하여 씬 뷰에서 개체를 선택할 수 있습니다.
		// 그러나 그래프 기즈모는 이를 방해합니다. 여기에서 메시를 그릴 경우 사용자는 뒤에 있는 개체를 선택할 수 없습니다.
		// (아마도 Unity는 대부분의 기즈모를 그리기 위해 Graphics.DrawMeshNow를 사용하고 있기 때문에
		// AstarPath 구성 요소와 기즈모를 연결하지 못할 것입니다). 실제로 씬 피킹이 발생하는 경우
		// Event.current.type이 'mouseUp'이 될 것으로 예상됩니다. 따라서 OnDrawGizmos 중에는
		// 임의 이벤트를 무시하여 기즈모가 씬 피킹과 상호 작용하지 않도록합니다.
		// 이러한 변경 사항이 화면에 영향을 미치는 경우에만 발생하기 때문에 시각적인 영향은 없습니다.
		// 테스트 결과 OnDrawGizmos 중에 발생할 수 있는 이벤트는 mouseUp 및 repaint 이벤트만 있는 것으로 보입니다.
		if (Event.current.type != EventType.Repaint) return;

		colorSettings.PushToStatic(this);

		AstarProfiler.StartProfile("OnDrawGizmos");

		if (workItems.workItemsInProgress || isScanning) {
			// 그래프 업데이트 중이므로 그래프 정보가 현재 유효하지 않을 수 있습니다.
			// 따라서 이전 프레임과 동일한 것을 그립니다.
			// 또한 씬에 여러 개의 카메라가 있거나 (또는 에디터에서 씬 뷰와 게임 뷰가 있는 경우) 우리는
			// 메쉬를 한 번 계산한 다음 다른 카메라에 대해 기존 메쉬를 다시 그립니다.
			// 이것은 성능을 상당히 향상시킵니다.
			gizmos.DrawExisting();
		} else {
			if (showNavGraphs && !manualDebugFloorRoof) {
				RecalculateDebugLimits();
			}

			Profiler.BeginSample("Graph.OnDrawGizmos");
			// 모든 그래프를 순환하고 기즈모를 그립니다.
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null && graphs[i].drawGizmos)
					graphs[i].OnDrawGizmos(gizmos, showNavGraphs);
			}
			Profiler.EndSample();

			if (showNavGraphs) {
				euclideanEmbedding.OnDrawGizmos();
				if (debugMode == GraphDebugMode.HierarchicalNode) hierarchicalGraph.OnDrawGizmos(gizmos);
			}
		}

		gizmos.FinalizeDraw();

		AstarProfiler.EndProfile("OnDrawGizmos");
	}

#if !ASTAR_NO_GUI
	/// <summary>
	/// InGame 디버깅을 그립니다 (활성화 된 경우), 'L' 키를 누르면 fps도 표시됩니다.
	/// See: <see cref="logPathResults"/> PathLog
	/// </summary>
	private void OnGUI () {
		if (logPathResults == PathLog.InGame && inGameDebugPath != "") {
			GUI.Label(new Rect(5, 5, 400, 600), inGameDebugPath);
		}
	}
#endif

	/// <summary>
	/// 경로 결과를 로그에 출력합니다. 출력 내용은 <see cref="logPathResults"/>를 사용하여 제어할 수 있습니다.
	/// See: <see cref="logPathResults"/>
	/// See: PathLog
	/// See: Pathfinding.Path.DebugString
	/// </summary>
	private void LogPathResults (Path path) {
		if (logPathResults != PathLog.None && (path.error || logPathResults != PathLog.OnlyErrors)) {
			string debug = (path as IPathInternals).DebugString(logPathResults);

			if (logPathResults == PathLog.InGame) {
				inGameDebugPath = debug;
			} else if (path.error) {
				//Debug.LogWarning(debug);
			} else {

			}
		}
	}

	/// <summary>
	/// 작업 항목이 실행되어야 하는지 확인한 다음 경로 찾기를 실행하고,
	/// 다음 요청한 스크립트로 계산된 경로를 반환합니다.
	///
	/// See: PerformBlockingActions
	/// See: PathProcessor.TickNonMultithreaded
	/// See: PathReturnQueue.ReturnPaths
	/// </summary>
	private void Update () {
		// 이 클래스는 [ExecuteInEditMode] 속성을 사용합니다.
		// 따라서 Update는 실행 중이지 않아도 호출됩니다.
		// 플레이 모드가 아닌 경우 아무 작업도 수행하지 마십시오.
		if (!Application.isPlaying) return;

		navmeshUpdates.Update();

		// 스캐닝 중이 아닌 경우 그래프 업데이트와 같은 블로킹 작업 실행
		if (!isScanning) {
			PerformBlockingActions();
		}

		// 다중 스레딩을 사용하지 않는 경우 경로 계산
		pathProcessor.TickNonMultithreaded();

		// 계산된 경로 반환
		pathReturnQueue.ReturnPaths(true);
	}

	private void PerformBlockingActions (bool force = false) {
		if (workItemLock.Held && pathProcessor.queue.AllReceiversBlocked) {
			// 블로킹 작업을 시작하기 전에 모든 경로를 반환합니다.
			// 이렇게하면 그래프가 변경되어 반환된 경로가 무효화될 수 있으므로(최소한 노드만)
			// 경로가 완료되었습니다.
			pathReturnQueue.ReturnPaths(false);

			Profiler.BeginSample("Work Items");
			if (workItems.ProcessWorkItems(force)) {
				// 이 단계에서 더 이상 작업 항목이 없으므로 경로 찾기 스레드를 다시 시작합니다.
				workItemLock.Release();
			}
			Profiler.EndSample();
		}
	}

	/// <summary>
	/// 경로 찾기가 일시 중지될 때 처리되도록 대기열에 작업 항목을 추가합니다.
	/// 편리한 메서드로 다음과 동일합니다.
	/// <code>
	/// AddWorkItem(new AstarWorkItem(callback));
	/// </code>
	///
	/// See: <see cref="AddWorkItem(AstarWorkItem)"/>
	/// </summary>
	public void AddWorkItem (System.Action callback) {
		AddWorkItem(new AstarWorkItem(callback));
	}

	/// <summary>
	/// 경로 찾기가 일시 중지될 때 처리되도록 대기열에 작업 항목을 추가합니다.
	/// 편리한 메서드로 다음과 동일합니다.
	/// <code>
	/// AddWorkItem(new AstarWorkItem(callback));
	/// </code>
	///
	/// See: <see cref="AddWorkItem(AstarWorkItem)"/>
	/// </summary>
	public void AddWorkItem (System.Action<IWorkItemContext> callback) {
		AddWorkItem(new AstarWorkItem(callback));
	}

	/// <summary>
	/// 경로 찾기가 일시 중지될 때 처리되도록 대기열에 작업 항목을 추가합니다.
	///
	/// 작업 항목은 노드를 업데이트하는 것이 안전한 경우에 실행됩니다. 이는 경로 검색 사이에서 정의됩니다.
	/// 더 많은 스레드를 사용하는 경우 이 메서드를 자주 호출하면 스레드에서 많은 대기시간으로 인해 경로 찾기 성능이 저하될 수 있습니다.
	/// 여기서 성능은 CPU 성능을 많이 사용하지 않는다는 의미가 아니라 초당 경로 수가 아마도 줄어 들 것입니다
	/// (그러나 프레임 속도는 약간 증가할 수 있음).
	///
	/// 이 함수는 주로 Unity의 메인 스레드에서만 호출해야 합니다 (즉, 일반적인 게임 코드).
	///
	/// <code>
	/// AstarPath.active.AddWorkItem(new AstarWorkItem(() => {
	///     // 여기에서 그래프를 업데이트해도 안전합니다.
	///     var node = AstarPath.active.GetNearest(transform.position).node;
	///     node.Walkable = false;
	/// }));
	/// </code>
	///
	/// <code>
	/// AstarPath.active.AddWorkItem(() => {
	///     // 여기에서 그래프를 업데이트해도 안전합니다.
	///     var node = AstarPath.active.GetNearest(transform.position).node;
	///     node.position = (Int3)transform.position;
	/// });
	/// </code>
	///
	/// See: <see cref="FlushWorkItems"/>
	/// </summary>
	public void AddWorkItem (AstarWorkItem item) {
		workItems.AddWorkItem(item);

		// 경로 찾기를 중지하고 작업 항목을 처리합니다.
		if (!workItemLock.Held) {
			workItemLock = PausePathfindingSoon();
		}

#if UNITY_EDITOR
		// 플레이 중이 아닌 경우 즉시 실행
		if (!Application.isPlaying) {
			FlushWorkItems();
		}
#endif
	}

	#region GraphUpdateMethods

	/// <summary>
	/// 가능한 한 빨리 대기 중인 그래프 업데이트를 적용합니다. <see cref="batchGraphUpdates"/>와 관계없이 호출됩니다.
	/// 여러 번 호출해도 여러 개의 콜백을 생성하지 않습니다.
	/// 이 함수는 그래프 업데이트 시간 제한과 관계없이 특정 그래프 업데이트를 가능한 한 빨리 적용하려는 경우 유용합니다.
	/// 참고로 이 함수는 업데이트가 완료될 때까지 차단되지 않으며, <see cref="batchGraphUpdates"/> 제한 시간을 우회하기만 합니다.
	///
	/// 참조: <see cref="FlushGraphUpdates"/>
	/// </summary>
	public void QueueGraphUpdates () {
		if (!graphUpdatesWorkItemAdded) {
			graphUpdatesWorkItemAdded = true;
			var workItem = graphUpdates.GetWorkItem();

			// 그래프 업데이트 작업 항목을 추가합니다. 먼저 graphUpdatesWorkItemAdded 플래그를 false로 설정한 다음 그래프 업데이트를 처리합니다.
			AddWorkItem(new AstarWorkItem(() => {
				graphUpdatesWorkItemAdded = false;
				lastGraphUpdate = Time.realtimeSinceStartup;

				workItem.init();
			}, workItem.update));
		}
	}

	/// <summary>
	/// 그래프 업데이트를 지연시켜 일정 시간 동안 대기합니다.
	/// batchGraphUpdates가 설정된 경우 경로 찾기 스레드를 계속 실행하고 큐에 대기 중인 호출을 한 번에 계산하려고 합니다.
	/// </summary>
	IEnumerator DelayedGraphUpdate () {
		graphUpdateRoutineRunning = true;

		yield return new WaitForSeconds(graphUpdateBatchingInterval-(Time.realtimeSinceStartup-lastGraphUpdate));
		QueueGraphUpdates();
		graphUpdateRoutineRunning = false;
	}

	/// <summary>
	/// 일정 시간 후에 bounds 내의 모든 그래프를 업데이트합니다.
	/// 그래프는 가능한 한 빨리 업데이트됩니다.
	///
	/// 참조: <see cref="FlushGraphUpdates"/>
	/// 참조: batchGraphUpdates
	/// 참조: graph-updates(작동 링크 온라인 설명서에서 보기)
	/// </summary>
	public void UpdateGraphs (Bounds bounds, float delay) {
		UpdateGraphs(new GraphUpdateObject(bounds), delay);
	}

	/// <summary>
	/// delay 초 후에 GraphUpdateObject를 사용하여 모든 그래프를 업데이트합니다.
	/// 이를 사용하여 예를 들어 지역 내의 모든 노드를 걷지 못하도록 만들거나 더 높은 패널티로 설정할 수 있습니다.
	///
	/// 참조: <see cref="FlushGraphUpdates"/>
	/// 참조: batchGraphUpdates
	/// 참조: graph-updates(작동 링크 온라인 설명서에서 보기)
	/// </summary>
	public void UpdateGraphs (GraphUpdateObject ob, float delay) {
		StartCoroutine(UpdateGraphsInternal(ob, delay));
	}

	/// <summary>일정 시간 후에 모든 그래프를 업데이트합니다.</summary>
	IEnumerator UpdateGraphsInternal (GraphUpdateObject ob, float delay) {
		yield return new WaitForSeconds(delay);
		UpdateGraphs(ob);
	}

	/// <summary>
	/// bounds 내의 모든 그래프를 업데이트합니다.
	/// 그래프는 가능한 한 빨리 업데이트됩니다.
	///
	/// 이것은 다음과 동일합니다.
	/// <code>
	/// UpdateGraphs(new GraphUpdateObject(bounds));
	/// </code>
	///
	/// 참조: <see cref="FlushGraphUpdates"/>
	/// 참조: batchGraphUpdates
	/// 참조: graph-updates(작동 링크 온라인 설명서에서 보기)
	/// </summary>
	public void UpdateGraphs (Bounds bounds) {
		UpdateGraphs(new GraphUpdateObject(bounds));
	}

	/// <summary>
	/// GraphUpdateObject를 사용하여 모든 그래프를 업데이트합니다.
	/// 예를 들어 지역 내의 모든 노드를 걷지 못하게 만들거나 더 높은 패널티로 설정하는 데 사용할 수 있습니다.
	/// 그래프는 가능한 한 빨리 (batchGraphUpdates에 따라) 업데이트됩니다.
	///
	/// 참조: <see cref="FlushGraphUpdates"/>
	/// 참조: batchGraphUpdates
	/// 참조: graph-updates(작동 링크 온라인 설명서에서 보기)
	/// </summary>
	public void UpdateGraphs (GraphUpdateObject ob) {
		if (ob.internalStage != GraphUpdateObject.STAGE_CREATED) {
			throw new System.Exception("You are trying to update graphs using the same graph update object twice. Please create a new GraphUpdateObject instead.");
		}
		ob.internalStage = GraphUpdateObject.STAGE_PENDING;
		graphUpdates.AddToQueue(ob);

		// 그래프 업데이트를 제한해야 하는 경우, 그래프를 업데이트해야 할 때까지 대기하는 코루틴을 시작합니다.
		if (batchGraphUpdates && Time.realtimeSinceStartup-lastGraphUpdate < graphUpdateBatchingInterval) {
			if (!graphUpdateRoutineRunning) {
				StartCoroutine(DelayedGraphUpdate());
			}
		} else {
			// 그렇지 않으면 그래프 업데이트는 가능한 한 빨리 수행되어야 합니다.
			QueueGraphUpdates();
		}
	}

	/// <summary>
	/// 그래프 업데이트를 단일 프레임에서 완료하도록 강제합니다.
	/// 이렇게 하면 경로 찾기 스레드가 현재 계산 중인 경로를 계산하도록 강제하고 일시 중지합니다 (있는 경우) .
	/// 모든 스레드가 일시 중지되면 그래프 업데이트가 수행됩니다.
	/// 경로 찾기 성능을 저하시킬 수 있는 (초당 많은 스레드가 서로를 기다리는 경우) 이 함수를 자주 (초당 여러 번) 사용하면 FPS가 감소할 수 있습니다.
	/// 그러나 실제로 그렇게 걱정할 필요는 없을 것입니다.
	///
	/// 참고: 이 함수는 거의 <see cref="FlushWorkItems"/>와 동일하지만 설명이 더 자세합니다.
	/// 이 함수는 그래프 업데이트에 대한 시간 제한 지연도 재정의합니다.
	/// 그 이유는 그래프 업데이트가 작업 항목을 사용하여 구현되기 때문입니다.
	/// 따라서 이 함수를 호출하면 다른 작업 항목 (있는 경우)도 실행됩니다.
	///
	/// 대기 중인 그래프 업데이트가 없으면 (다른 작업 항목도 실행되지 않음) 아무 작업도 수행하지 않습니다.
	/// </summary>
	public void FlushGraphUpdates () {
		if (IsAnyGraphUpdateQueued) {
			QueueGraphUpdates();
			FlushWorkItems();
		}
	}

	#endregion

	/// <summary>
	/// 현재 프레임에서 모든 작업 항목을 완료하도록 강제합니다.
	/// 이렇게 하면 모든 작업 항목이 즉시 실행됩니다.
	/// 경로 찾기 스레드가 현재 계산 중인 경로를 계산하도록 강제하고 일시 중지합니다 (있는 경우).
	/// 모든 스레드가 일시 중지되면 작업 항목이 실행됩니다 (예: 그래프 업데이트).
	///
	/// 경고: 이것을 자주 (초당 많은 번) 사용하면 서로를 기다리는 많은 스레드 때문에 FPS가 감소할 수 있습니다.
	/// 그러나 아마도 그렇게 걱정할 필요는 없을 것입니다.
	///
	/// 참고: 이것은 거의 (<see cref="FlushGraphUpdates"/>와 거의 동일함) 동일하지만 설명이 더 자세합니다.
	///
	/// 대기 중인 실행할 작업 항목이 없으면 아무 작업도 수행하지 않습니다.
	/// </summary>
	public void FlushWorkItems () {
		if (workItems.anyQueued) {
			var graphLock = PausePathfinding();
			PerformBlockingActions(true);
			graphLock.Release();
		}
	}

	/// <summary>
	/// 작업 항목이 실행되도록 확인합니다.
	///
	/// 참조: AddWorkItem
	///
	/// Deprecated: 대신 <see cref="FlushWorkItems()"/>를 사용합니다.
	/// </summary>
	/// <param name="unblockOnComplete">true인 경우 작업 항목을 모두 완료한 후에 즉시 경로 찾기를 시작할 수 있습니다.</param>
	/// <param name="block">true인 경우 일반적으로 여러 프레임에 걸쳐 완료되는 작업 항목을이 호출 중에 완료하도록 강제합니다.
	///              false이면이 호출 후에 아직 작업이 남아 있을 수 있습니다.</param>
	[System.Obsolete("FlushWorkItems() 대신 사용하세요")]
	public void FlushWorkItems (bool unblockOnComplete, bool block) {
		var graphLock = PausePathfinding();

		// Run tasks
		PerformBlockingActions(block);
		graphLock.Release();
	}

	/// <summary>
	/// 사용할 스레드 수를 계산합니다.
	/// count가 자동이 아닌 경우, count를 int로 캐스팅하여 반환합니다.
	/// 반환 값: 사용할 스레드 수를 지정하는 int입니다. 0은 별도의 스레드 대신 경로 찾기에 대기열을 사용해야 함을 의미합니다.
	///
	/// count가 Automatic로 설정된 경우, 현재 시스템의 프로세서 수와 메모리에 기반한 값을 반환합니다.
	/// 메모리가 <= 512MB 이거나 논리 코어가 <= 1이면 0을 반환합니다. 메모리가 <= 1024이면 스레드를 최대 2개로 클램핑합니다.
	/// 그렇지 않으면 논리 코어 수를 6으로 클램핑합니다.
	///
	/// WebGL에서는 이 메서드는 항상 0을 반환합니다.
	/// </summary>
	public static int CalculateThreadCount (ThreadCount count) {
#if UNITY_WEBGL
		return 0;
#else
		if (count == ThreadCount.AutomaticLowLoad || count == ThreadCount.AutomaticHighLoad) {
			int logicalCores = Mathf.Max(1, SystemInfo.processorCount);
			int memory = SystemInfo.systemMemorySize;

			if (memory <= 0) {
				Debug.LogError("Machine reporting that is has <= 0 bytes of RAM. This is definitely not true, assuming 1 GiB");
				memory = 1024;
			}

			if (logicalCores <= 1) return 0;
			if (memory <= 512) return 0;

			return 1;
		} else {
			return (int)count > 0 ? 1 : 0;
		}
#endif
	}

	/// <summary>
	/// 필요한 변수를 설정하고 그래프를 스캔합니다.
	/// Initialize를 호출하고 ReturnPaths 코루틴을 시작하고 모든 그래프를 스캔합니다.
	/// 또한 멀티스레딩을 사용하는 경우 스레드를 시작합니다.
	/// 참조: <see cref="OnAwakeSettings"/>
	/// </summary>
	protected override void Awake () {
		base.Awake();
		// 싱글톤 패턴이 유지되도록 매우 중요합니다.
		active = this;

		if (FindObjectsOfType(typeof(AstarPath)).Length > 1) {
			Debug.LogError("한 번에 하나 이상의 AstarPath 구성 요소를 씬에 두지 말아야 합니다.\n" +
		  "이렇게 하면 AstarPath 구성 요소가 싱글톤 패턴을 중심으로 빌드되기 때문에 심각한 오류가 발생할 수 있습니다.");
		}

		// GUILayout를 사용하지 않도록 설정하여 성능을 높입니다. OnGUI 호출에서 사용되지 않습니다.
		useGUILayout = false;

		// 이 클래스는 [ExecuteInEditMode] 속성을 사용하므로 Awake는 실행 중이지 않을 때도 호출됩니다.
		// 플레이 모드가 아닐 때는 아무 작업도 수행하지 않습니다.
		if (!Application.isPlaying) return;

		if (OnAwakeSettings != null) {
			OnAwakeSettings();
		}

		// 스캔하기 전에 모든 그래프 수정자가 활성화되었는지 확인 (스크립트 실행 순서 문제 방지)
		GraphModifier.FindAllModifiers();
		RelevantGraphSurface.FindAllGraphSurfaces();

		InitializePathProcessor();
		InitializeProfiler();
		ConfigureReferencesInternal();
		InitializeAstarData();

		// 작업 항목 플러시, 그래프 데이터 로드를위한 InitializeAstarData에 추가 될 수 있음
		FlushWorkItems();

		euclideanEmbedding.dirty = true;

		navmeshUpdates.OnEnable();

		if (scanOnStartup && (!data.cacheStartup || data.file_cachedStartup == null)) {
			Scan();
		}
	}

	/// <summary><see cref="pathProcessor"/> 필드를 초기화합니다.</summary>
	void InitializePathProcessor () {
		int numThreads = CalculateThreadCount(threadCount);

		// 플레이 모드 이외에서는 모든 것이 동기적이므로 스레드를 사용하지 않습니다.
		if (!Application.isPlaying) numThreads = 0;

		// 단순한 모딩으로 여러 스레드를 지원하려는 시도 방지
		if (numThreads > 1) {
			threadCount = ThreadCount.One;
			numThreads = 1;
		}

		int numProcessors = Mathf.Max(numThreads, 1);
		bool multithreaded = numThreads > 0;
		pathProcessor = new PathProcessor(this, pathReturnQueue, numProcessors, multithreaded);

		pathProcessor.OnPathPreSearch += path => {
			var tmp = OnPathPreSearch;
			if (tmp != null) tmp(path);
		};

		pathProcessor.OnPathPostSearch += path => {
			LogPathResults(path);
			var tmp = OnPathPostSearch;
			if (tmp != null) tmp(path);
		};

		// Sent every time the path queue is unblocked
		pathProcessor.OnQueueUnblocked += () => {
			if (euclideanEmbedding.dirty) {
				euclideanEmbedding.RecalculateCosts();
			}
		};

		if (multithreaded) {
			graphUpdates.EnableMultithreading();
		}
	}

	/// <summary>간단한 오류 확인을 수행합니다.</summary>
	internal void VerifyIntegrity () {
		if (active != this)
		{
			throw new System.Exception("싱글톤 패턴이 깨졌습니다. 씬에 AstarPath 개체를 하나만 두십시오.");
		}

		if (data == null)
		{
			throw new System.NullReferenceException("데이터가 null입니다... A*가 제대로 설정되지 않았습니까?");
		}

		if (data.graphs == null) {
			data.graphs = new NavGraph[0];
			data.UpdateShortcuts();
		}
	}

	/// <summary>\cond internal</summary>
	/// <summary>
	/// <see cref="active"/>가 이 개체로 설정되고 <see cref="data"/>가 null이 아닌지 확인하는 내부 메서드입니다.
	/// 또한 <see cref="colorSettings"/>의 OnEnable을 호출하고 데이터.userConnections를 초기화하지 않은 경우 초기화합니다.
	///
	/// 경고: 주로 시스템 내부에서 사용하도록 되어 있습니다.
	/// </summary>
	public void ConfigureReferencesInternal () {
		active = this;
		data = data ?? new AstarData();
		colorSettings = colorSettings ?? new AstarColor();
		colorSettings.PushToStatic(this);
	}
	/// <summary>\endcond</summary>

	/// <summary>AstarProfiler.InitializeFastProfile를 호출합니다.</summary>
	void InitializeProfiler () {
		AstarProfiler.InitializeFastProfile(new string[14] {
			"Prepare",          //0
			"Initialize",       //1
			"CalculateStep",    //2
			"Trace",            //3
			"Open",             //4
			"UpdateAllG",       //5
			"Add",              //6
			"Remove",           //7
			"PreProcessing",    //8
			"Callback",         //9
			"Overhead",         //10
			"Log",              //11
			"ReturnPaths",      //12
			"PostPathCallback"  //13
		});
	}

	/// <summary>
	/// AstarData 클래스를 초기화합니다.
	/// 그래프 유형을 검색하고 <see cref="data"/> 및 모든 그래프의 Awake를 호출합니다.
	///
	/// 참조: AstarData.FindGraphTypes
	/// </summary>
	void InitializeAstarData () {
		data.FindGraphTypes();
		data.Awake();
		data.UpdateShortcuts();
	}

	/// <summary>메모리 누수를 방지하기 위해 메시를 정리합니다.</summary>
	void OnDisable () {
		gizmos.ClearCache();
	}

	/// <summary>
	/// 변수 및 기타 항목을 정리하고 그래프를 제거합니다.
	/// AstarPath 개체를 파괴할 때 모든 콜백과 같은 정적 변수가 지워집니다.
	/// </summary>
	void OnDestroy () {
		// 이 클래스는 [ExecuteInEditMode] 속성을 사용하므로 OnDestroy는 실행 중이지 않을 때도 호출됩니다.
		// 플레이 모드가 아닐 때는 아무 작업도 수행하지 않습니다.
		if (!Application.isPlaying) return;

		if (logPathResults == PathLog.Heavy)
			Debug.Log("+++ AstarPath Component Destroyed - Cleaning Up Pathfinding Data +++");

		if (active != this) return;

		// 현재 경로 계산이 완료 될 때까지 경로 찾기 스레드 차단
		PausePathfinding();

		navmeshUpdates.OnDisable();

		euclideanEmbedding.dirty = false;
		FlushWorkItems();

		// 이 AstarPath 인스턴스에 더 이상 경로 호출을 수용하지 않습니다.
		// 이로 인해 모든 경로 찾기 스레드가 종료됩니다 (있는 경우)
		pathProcessor.queue.TerminateReceivers();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Processing Possible Work Items");

		// 그래프 업데이트 스레드 중지 (실행 중인 경우)
		graphUpdates.DisableMultithreading();

		// 경로 찾기 스레드 가입 시도
		pathProcessor.JoinThreads();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Returning Paths");


		// 모든 경로 반환
		pathReturnQueue.ReturnPaths(false);

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Destroying Graphs");


		// 그래프 데이터 정리
		data.OnDestroy();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Cleaning up variables");

		// 변수 정리, 정적 변수는 정리해야 다음 씬에서 이상한 데이터를 받지 않을 것입니다.

		// 모든 콜백 지우기
		OnAwakeSettings			= null;
		OnGraphPreScan          = null;
		OnGraphPostScan         = null;
		OnPathPreSearch         = null;
		OnPathPostSearch        = null;
		OnPreScan               = null;
		OnPostScan              = null;
		OnLatePostScan          = null;
		On65KOverflow           = null;
		OnGraphsUpdated         = null;

		active = null;
	}

	#region 스캔 메서드

	/// <summary>
	/// 새로운 전역 노드 인덱스를 반환합니다.
	/// 경고: 이 메서드는 직접 호출해서는 안됩니다. GraphNode 생성자에서 사용됩니다.
	/// </summary>
	internal int GetNewNodeIndex () {
		return pathProcessor.GetNewNodeIndex();
	}

	/// <summary>
	/// 노드의 임시 경로 데이터를 초기화합니다.
	/// 경고: 이 메서드는 직접 호출해서는 안됩니다. GraphNode 생성자에서 사용됩니다.
	/// </summary>
	internal void InitializeNode (GraphNode node) {
		pathProcessor.InitializeNode(node);
	}

	/// <summary>
	/// 주어진 노드를 파괴하기 위한 내부 메서드입니다.
	/// 이 메서드는 노드가 그래프에서 분리된 후에 호출되어 다른 노드에서 도달할 수 없도록합니다.
	/// 이 메서드는 그래프 업데이트 중에만 호출되어야 합니다. 즉, 경로 찾기 스레드가 실행 중이지 않거나 일시 중지된 경우에만 호출되어야 합니다.
	///
	/// 경고: 사용자 코드에서 이 메서드를 직접 호출해서는 안됩니다. 내부적으로 시스템에서 사용됩니다.
	/// </summary>
	internal void DestroyNode (GraphNode node) {
		pathProcessor.DestroyNode(node);
	}

	/// <summary>
	/// 모든 경로 찾기 스레드가 일시 중지되고 차단 될 때까지 차단합니다.
	///
	/// <code>
	/// var graphLock = AstarPath.active.PausePathfinding();
	/// // 여기에서 그래프를 안전하게 수정할 수 있습니다. 예를 들어 포인트 그래프에 새로운 노드를 추가합니다.
	/// var node = AstarPath.active.data.pointGraph.AddNode((Int3) new Vector3(3, 1, 4));
	///
	/// // 경로 찾기를 재개하려면
	/// graphLock.Release();
	/// </code>
	///
	/// 반환: 잠금 개체입니다. 경로 찾기를 다시 시작하려면 <see cref="Pathfinding.PathProcessor.GraphUpdateLock.Release"/>를 호출해야 합니다.
	/// 참고: 대부분의 경우 사용자 코드에서 직접 호출해서는 안됩니다. 대신 <see cref="AddWorkItem"/> 메서드를 사용하십시오.
	///
	/// 참조: <see cref="AddWorkItem"/>
	/// </summary>
	public PathProcessor.GraphUpdateLock PausePathfinding () {
		return pathProcessor.PausePathfinding(true);
	}

	/// <summary>경로 큐를 차단하여 작업 항목을 수행합니다.</summary>
	PathProcessor.GraphUpdateLock PausePathfindingSoon () {
		return pathProcessor.PausePathfinding(false);
	}

	/// <summary>
	/// 특정 그래프를 스캔합니다.
	/// 이 메서드를 호출하면 지정된 그래프를 다시 계산합니다.
	/// 이 메서드는 상당히 느릴 수 있으므로 (그래프 유형 및 그래프 복잡성에 따라 다릅니다) 가능한 작은 그래프 업데이트를 사용하는 것이 좋습니다.
	///
	/// <code>
	/// // 모든 그래프 다시 계산
	/// AstarPath.active.Scan();
	///
	/// // 첫 번째 그리드 그래프만 다시 계산
	/// var graphToScan = AstarPath.active.data.gridGraph;
	/// AstarPath.active.Scan(graphToScan);
	///
	/// // 첫 번째 및 세 번째 그래프만 다시 계산
	/// var graphsToScan = new [] { AstarPath.active.data.graphs[0], AstarPath.active.data.graphs[2] };
	/// AstarPath.active.Scan(graphsToScan);
	/// </code>
	///
	/// 참조: 그래프 업데이트 (온라인 문서에서 작동 링크 확인)
	/// 참조: ScanAsync
	/// </summary>
	public void Scan (NavGraph graphToScan) {
		if (graphToScan == null) throw new System.ArgumentNullException();
		Scan(new NavGraph[] { graphToScan });
	}

	/// <summary>
	/// 지정된 모든 그래프를 스캔합니다.
	///
	/// 이 메서드를 호출하면 지정된 모든 그래프 또는 graphsToScan 매개변수가 null인 경우 모든 그래프를 다시 계산합니다.
	/// 이 메서드는 상당히 느릴 수 있으므로 (그래프 유형 및 그래프 복잡성에 따라 다릅니다) 가능한 작은 그래프 업데이트를 사용하는 것이 좋습니다.
	///
	/// <code>
	/// // 모든 그래프 다시 계산
	/// AstarPath.active.Scan();
	///
	/// // 첫 번째 그리드 그래프만 다시 계산
	/// var graphToScan = AstarPath.active.data.gridGraph;
	/// AstarPath.active.Scan(graphToScan);
	///
	/// // 첫 번째 및 세 번째 그래프만 다시 계산
	/// var graphsToScan = new [] { AstarPath.active.data.graphs[0], AstarPath.active.data.graphs[2] };
	/// AstarPath.active.Scan(graphsToScan);
	/// </code>
	///
	/// 참조: 그래프 업데이트 (온라인 문서에서 작동 링크 확인)
	/// 참조: ScanAsync
	/// </summary>
	/// <param name="graphsToScan">스캔할 그래프입니다. 이 매개변수가 null인 경우 모든 그래프가 스캔됩니다.</param>
	public void Scan (NavGraph[] graphsToScan = null) {
		var prevProgress = new Progress();

		Profiler.BeginSample("Scan");
		Profiler.BeginSample("Init");
		foreach (var p in ScanAsync(graphsToScan)) {
			if (prevProgress.description != p.description) {
#if !NETFX_CORE && UNITY_EDITOR
				Profiler.EndSample();
				Profiler.BeginSample(p.description);
				// Log progress to the console
				System.Console.WriteLine(p.description);
				prevProgress = p;
#endif
			}
		}
		Profiler.EndSample();
		Profiler.EndSample();
	}

	/// <summary>
	/// 특정 그래프를 비동기로 스캔합니다. 이는 IEnumerable이므로 진행률을 얻으려면 루프를 통과할 수 있습니다.
	/// <code>
	/// foreach (Progress progress in AstarPath.active.ScanAsync()) {
	///     Debug.Log("Scanning... " + progress.description + " - " + (progress.progress*100).ToString("0") + "%");
	/// }
	/// </code>
	/// 진행률을 통과할 때 비동기로 그래프를 스캔할 수 있습니다.
	/// 이것은 좋은 프레임 속도를 보장하지는 않지만 스캔 중에 진행률 바를 표시할 수 있게 합니다.
	/// <code>
	/// IEnumerator Start () {
	///     foreach (Progress progress in AstarPath.active.ScanAsync()) {
	///         Debug.Log("Scanning... " + progress.description + " - " + (progress.progress*100).ToString("0") + "%");
	///         yield return null;
	///     }
	/// }
	/// </code>
	///
	/// 참조: Scan
	/// </summary>
	public IEnumerable<Progress> ScanAsync (NavGraph graphToScan) {
		if (graphToScan == null) throw new System.ArgumentNullException();
		return ScanAsync(new NavGraph[] { graphToScan });
	}

	/// <summary>
	/// 지정된 모든 그래프를 비동기로 스캔합니다. 이는 IEnumerable이므로 진행률을 얻으려면 루프를 통과할 수 있습니다.
	///
	/// <code>
	/// foreach (Progress progress in AstarPath.active.ScanAsync()) {
	///     Debug.Log("Scanning... " + progress.description + " - " + (progress.progress*100).ToString("0") + "%");
	/// }
	/// </code>
	/// 진행률을 통과할 때 비동기로 그래프를 스캔할 수 있습니다.
	/// 이것은 좋은 프레임 속도를 보장하지는 않지만 스캔 중에 진행률 바를 표시할 수 있게 합니다.
	/// <code>
	/// IEnumerator Start () {
	///     foreach (Progress progress in AstarPath.active.ScanAsync()) {
	///         Debug.Log("Scanning... " + progress.description + " - " + (progress.progress*100).ToString("0") + "%");
	///         yield return null;
	///     }
	/// }
	/// </code>
	///
	/// 참조: Scan
	/// </summary>
	/// <param name="graphsToScan">스캔할 그래프입니다. 이 매개변수가 null인 경우 모든 그래프가 스캔됩니다.</param>
	public IEnumerable<Progress> ScanAsync (NavGraph[] graphsToScan = null) {
		if (graphsToScan == null) graphsToScan = graphs;

		if (graphsToScan == null) {
			yield break;
		}

		if (isScanning) throw new System.InvalidOperationException("다른 비동기 스캔이 이미 실행 중입니다");

		isScanning = true;

		VerifyIntegrity();

		var graphUpdateLock = PausePathfinding();

		// 큐에 반환 될 모든 경로가 즉시 반환되도록 합니다
		// 일부 변경기 (예: funnel 변경기)는 경로가 반환 될 때 노드가 유효한 것으로 의존하기 때문입니다.
		pathReturnQueue.ReturnPaths(false);

		if (!Application.isPlaying) {
			data.FindGraphTypes();
			GraphModifier.FindAllModifiers();
		}

		int startFrame = Time.frameCount;

		yield return new Progress(0.05F, "Pre processing graphs");

		// 네,이 제한은 무시하기 쉬움
		// 코드는 무료 버전과 무료 버전을 위한 별도의 코드를 갖는 것이 귀찮습니다.
		// 여기서 무엇을 할 수 있는지 즐겨 주시면 감사하겠습니다.
		// A * 경로 찾기 프로젝트의 프로 버전을 구입해 주시기 바랍니다.
		if (Time.frameCount != startFrame) {
			throw new System.Exception("비동기 스캔은 A * 경로 찾기 프로젝트의 프로 버전에서만 수행할 수 있습니다.");
		}

		if (OnPreScan != null) {
			OnPreScan(this);
		}

		GraphModifier.TriggerEvent(GraphModifier.EventType.PreScan);

		data.LockGraphStructure();

		Physics2D.SyncTransforms();
		var watch = System.Diagnostics.Stopwatch.StartNew();

		// 이전 노드를 파괴합니다.
		for (int i = 0; i < graphsToScan.Length; i++) {
			if (graphsToScan[i] != null) {
				((IGraphInternals)graphsToScan[i]).DestroyAllNodes();
			}
		}

		// 그래프를 하나씩 루프를 돌며 스캔합니다.
		for (int i = 0; i < graphsToScan.Length; i++) {
			// 널 그래프 건너 뜁니다.
			if (graphsToScan[i] == null) continue;

			// 진행 정보를위한 것
			// 이 그래프는 진행 막대를 minp에서 maxp로 이동시킵니다.
			float minp = Mathf.Lerp(0.1F, 0.8F, (float)(i)/(graphsToScan.Length));
			float maxp = Mathf.Lerp(0.1F, 0.8F, (float)(i+0.95F)/(graphsToScan.Length));

			var progressDescriptionPrefix = "Scanning graph " + (i+1) + " of " + graphsToScan.Length + " - ";

			// 예외 처리 때문에 약간 복잡해지는 예외 처리 때문에 foreach 루프와 유사하지만
			// (try-except 절 내에서 yield 할 수 없음) 예외 처리입니다.
			var coroutine = ScanGraph(graphsToScan[i]).GetEnumerator();
			while (true) {
				try {
					if (!coroutine.MoveNext()) break;
				} catch {
					isScanning = false;
					data.UnlockGraphStructure();
					graphUpdateLock.Release();
					throw;
				}
				yield return coroutine.Current.MapTo(minp, maxp, progressDescriptionPrefix);
			}
		}

		data.UnlockGraphStructure();
		yield return new Progress(0.8F, "Post processing graphs");

		if (OnPostScan != null) {
			OnPostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PostScan);

		FlushWorkItems();

		yield return new Progress(0.9F, "Computing areas");

		hierarchicalGraph.RecalculateIfNecessary();

		yield return new Progress(0.95F, "Late post processing");

		// 여기에서 스캔을 중지 한 것을 신호로 보냅니다
		// 이 지점 이후에는 어떤 수익도 일어나서는 안됩니다.
		// 다른 시스템의 일부가 간섭하기 시작하기 때문입니다.
		isScanning = false;

		if (OnLatePostScan != null) {
			OnLatePostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.LatePostScan);

		euclideanEmbedding.dirty = true;
		euclideanEmbedding.RecalculatePivots();

		// 차단 작업 수행
		FlushWorkItems();
		// 경로 찾기 스레드 재개
		graphUpdateLock.Release();

		watch.Stop();
		lastScanTime = (float)watch.Elapsed.TotalSeconds;

		System.GC.Collect();

		if (logPathResults != PathLog.None && logPathResults != PathLog.OnlyErrors) {
			//Debug.Log("Scanning - Process took "+(lastScanTime*1000).ToString("0")+" ms to complete");
		}
	}

	IEnumerable<Progress> ScanGraph (NavGraph graph) {
		if (OnGraphPreScan != null) {
			yield return new Progress(0, "Pre processing");
			OnGraphPreScan(graph);
		}

		yield return new Progress(0, "");

		foreach (var p in ((IGraphInternals)graph).ScanInternal()) {
			yield return p.MapTo(0, 0.95f);
		}

		yield return new Progress(0.95f, "Assigning graph indices");

		// 그래프 내 모든 노드에 그래프 인덱스 할당
		graph.GetNodes(node => node.GraphIndex = (uint)graph.graphIndex);

		if (OnGraphPostScan != null) {
			yield return new Progress(0.99f, "Post processing");
			OnGraphPostScan(graph);
		}
	}

	#endregion

	private static int waitForPathDepth = 0;

	/// <summary>
	/// 경로가 계산될 때까지 차단됩니다.
	///
	/// 일반적으로 경로가 계산되고 반환되기까지 몇 프레임이 소요됩니다.
	/// 이 함수는 함수가 반환되면 경로가 계산되고 해당 경로에 대한 콜백이 호출되도록 보장합니다.
	///
	/// 여러 경로를 한 번에 요청하고 마지막 경로가 완료될 때까지 대기하는 경우,
	/// 큐에 있는 경로 대부분을 계산합니다 (멀티스레딩을 사용하는 경우 대부분만 계산되며,
	/// 멀티스레딩을 사용하지 않는 경우 모두 계산됩니다).
	///
	/// 이 함수는 실제로 필요한 경우에만 사용하십시오.
	/// 경로 계산을 여러 프레임에 걸쳐 분산하는 것이 좋습니다.
	/// 이렇게 하면 프레임률을 부드럽게 유지하고 동시에 여러 경로를 동시에 요청해도 랙이 발생하지 않습니다.
	///
	/// 참고: 이 함수의 실행 중에 그래프 업데이트 및 기타 콜백이 호출될 수 있습니다.
	///
	/// 패스파인더가 종료 중인 경우. 즉, OnDestroy에서는 이 함수가 아무 작업도 수행하지 않습니다.
	///
	/// \throws Exception 경로를 대기하는 동안 경로 찾기가 이 장면에서 올바르게 초기화되지 않았거나 (아마도 AstarPath 개체가 없음)
	/// 또는 경로가 아직 시작되지 않았을 때 예외가 발생합니다.
	/// 일반적인 경우 발생하지 않도록 경로파인딩 스레드가 충돌한 경우와 같은 심각한 오류가 발생하면 예외가 발생합니다 (보통 일반적인 경우에는 발생하지 않아야 합니다).
	/// 경로 대기 중 무한 루프를 방지하기 위한 조치입니다.
	///
	/// 참조: Pathfinding.Path.WaitForPath
	/// 참조: Pathfinding.Path.BlockUntilCalculated
	/// </summary>
	/// <param name="path">대기할 경로. 경로가 시작되지 않았다면 예외가 발생합니다.</param>
	public static void BlockUntilCalculated (Path path) {
		if (active == null)
			throw new System.Exception("이 장면에서 경로 찾기가 올바르게 초기화되지 않았습니다. " +
		   "AstarPath.active가 null입니다.\nAwake에서 이 함수를 호출하지 마십시오.");

		if (path == null) throw new System.ArgumentNullException("경로는 null이 될 수 없습니다");

		if (active.pathProcessor.queue.IsTerminating) return;

		if (path.PipelineState == PathState.Created)
		{
			throw new System.Exception("지정된 경로가 아직 시작되지 않았습니다.");
		}

		waitForPathDepth++;

		if (waitForPathDepth == 5)
		{
			Debug.LogError("재귀적으로 BlockUntilCalculated 함수를 호출하고 있습니다 (아마도 경로 콜백에서). 이렇게 하지 마십시오.");
		}

		if (path.PipelineState < PathState.ReturnQueue) {
			if (active.IsUsingMultithreading) {
				while (path.PipelineState < PathState.ReturnQueue) {
					if (active.pathProcessor.queue.IsTerminating)
					{
						waitForPathDepth--;
						throw new System.Exception("패스파인딩 스레드가 충돌한 것으로 보입니다.");
					}

					// 스레드가 경로를 계산하기를 기다립니다
					Thread.Sleep(1);
					active.PerformBlockingActions(true);
				}
			} else {
				while (path.PipelineState < PathState.ReturnQueue) {
					if (active.pathProcessor.queue.IsEmpty && path.PipelineState != PathState.Processing)
					{
						waitForPathDepth--;
						throw new System.Exception("심각한 오류입니다. 경로 큐가 비어 있지만 경로 상태는 '" + path.PipelineState + "'입니다.");
					}

					// 일부 경로 계산
					active.pathProcessor.TickNonMultithreaded();
					active.PerformBlockingActions(true);
				}
			}
		}

		active.pathReturnQueue.ReturnPaths(false);
		waitForPathDepth--;
	}

	/// <summary>
	/// 경로를 가능한 빨리 계산되도록 대기열에 추가합니다.
	/// 경로가 계산될 때 지정된 콜백이 호출됩니다.
	/// 일반적으로 직접 이 함수를 호출하는 대신 Seeker 컴포넌트를 사용해야 합니다.
	/// </summary>
	/// <param name="path">대기열에 추가할 경로입니다.</param>
	/// <param name="pushToFront">true인 경우 경로가 대기열의 앞쪽으로 푸시되어 대기 중인 다른 경로를 우회하고 다음에 계산됩니다.
	/// 이것은 다른 경로보다 우선하여 계산하려는 경로가 있을 때 유용할 수 있습니다. 그러나 과도하게 사용하면 조심해야 합니다.
	/// 너무 많은 경로가 자주 앞쪽에 푸시되면 경로가 계산되기까지 다른 경로가 아주 오랜 시간을 기다릴 수 있습니다.</param>
	public static void StartPath (Path path, bool pushToFront = false) {
		// 다중스레딩 문제를 피하기 위해 로컬 변수에 복사합니다.
		var astar = active;

		if (System.Object.ReferenceEquals(astar, null))
		{
			Debug.LogError("이 장면에는 AstarPath 개체가 없거나 아직 초기화되지 않았습니다.");
			return;
		}

		if (path.PipelineState != PathState.Created)
		{
			throw new System.Exception("경로의 상태가 잘못되었습니다. " + PathState.Created + " 상태가 예상되었지만 " + path.PipelineState + " 상태를 찾았습니다.\n" +
				"동일한 경로를 두 번 요청하지 않도록 확인하십시오.");
		}

		if (astar.pathProcessor.queue.IsTerminating)
		{
			path.FailWithError("새 경로는 허용되지 않습니다");
			return;
		}

		if (astar.graphs == null || astar.graphs.Length == 0)
		{
			Debug.LogError("장면에 그래프가 없습니다");
			path.FailWithError("장면에 그래프가 없습니다");
			Debug.LogError(path.errorLog);
			return;
		}

		path.Claim(astar);

		// 상태를 PathState.PathQueue로 증가시킵니다
		((IPathInternals)path).AdvanceState(PathState.PathQueue);
		if (pushToFront) {
			astar.pathProcessor.queue.PushFront(path);
		} else {
			astar.pathProcessor.queue.Push(path);
		}

		// 플레이 모드 외부에서는 모든 경로 요청이 동기적입니다
		if (!Application.isPlaying) {
			BlockUntilCalculated(path);
		}
	}

	/// <summary>
	/// 불필요한 할당을 피하기 위해 NNConstraint.None을 캐시합니다.
	/// 이 문제는 NNConstraint를 불변 클래스/구조체로 만들어서 이상적으로 수정해야 합니다.
	/// </summary>
	static readonly NNConstraint NNConstraintNone = NNConstraint.None;

	/// <summary>
	/// 위치에 가장 가까운 노드를 반환합니다.
	/// 이 메서드는 모든 그래프를 검색하여 해당 위치에 가장 가까운 노드를 선택하고 반환합니다.
	///
	/// GetNearest(position, NNConstraint.None)과 동등합니다.
	///
	/// <code>
	/// // 이 게임 오브젝트 위치에 가장 가까운 노드를 찾습니다.
	/// GraphNode node = AstarPath.active.GetNearest(transform.position).node;
	///
	/// if (node.Walkable) {
	///     // 노드가 걷기 가능하면 여기에 타워를 설치하거나 기타 작업을 수행할 수 있습니다.
	/// }
	/// </code>
	///
	/// 참조: Pathfinding.NNConstraint
	/// </summary>
	public NNInfo GetNearest (Vector3 position) {
		return GetNearest(position, NNConstraintNone);
	}

	/// <summary>
	/// 지정된 NNConstraint를 사용하여 위치에 가장 가까운 노드를 반환합니다.
	/// 모든 그래프를 검색하여 지정된 위치에 가장 가까운 노드를 선택합니다.
	/// NNConstraint는 걷기 가능한 노드만 선택하는 등 어떤 노드를 선택할지 제약을 설정하는 데 사용할 수 있습니다.
	///
	/// <code>
	/// GraphNode node = AstarPath.active.GetNearest(transform.position, NNConstraint.Default).node;
	/// </code>
	///
	/// <code>
	/// var constraint = NNConstraint.None;
	///
	/// // 걷기 가능한 노드만 검색으로 제약 설정
	/// constraint.constrainWalkability = true;
	/// constraint.walkable = true;
	///
	/// // 태그 3 또는 태그 5인 노드만 검색으로 제약 설정
	/// // 'tags' 필드는 비트마스크입니다.
	/// constraint.constrainTags = true;
	/// constraint.tags = (1 << 3) | (1 << 5);
	///
	/// var info = AstarPath.active.GetNearest(transform.position, constraint);
	/// var node = info.node;
	/// var closestPoint = info.position;
	/// </code>
	///
	/// 참조: Pathfinding.NNConstraint
	/// </summary>
	public NNInfo GetNearest (Vector3 position, NNConstraint constraint) {
		return GetNearest(position, constraint, null);
	}

	/// <summary>
	/// 지정된 NNConstraint를 사용하여 위치에 가장 가까운 노드를 반환합니다.
	/// 모든 그래프를 검색하여 지정된 위치에 가장 가까운 노드를 선택합니다.
	/// NNConstraint는 걷기 가능한 노드만 선택하는 등 어떤 노드를 선택할지 제약을 설정하는 데 사용할 수 있습니다.
	/// 참조: Pathfinding.NNConstraint
	/// </summary>
	public NNInfo GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
		// 속성 조회를 캐시합니다.
		var graphs = this.graphs;

		float minDist = float.PositiveInfinity;
		NNInfoInternal nearestNode = new NNInfoInternal();
		int nearestGraph = -1;

		if (graphs != null) {
			for (int i = 0; i < graphs.Length; i++) {
				NavGraph graph = graphs[i];

				// 이 그래프를 검색해야 하는지 확인합니다.
				if (graph == null || !constraint.SuitableGraph(i, graph)) {
					continue;
				}

				NNInfoInternal nnInfo;
				if (fullGetNearestSearch) {
					// 느린 가장 가까운 노드 검색
					// 이는 제약에 따라 적합한 노드를 찾으려고 시도합니다.
					nnInfo = graph.GetNearestForce(position, constraint);
				} else {
					// 빠른 가장 가까운 노드 검색
					// 제약을 크게 사용하지 않고 위치에 가까운 노드를 찾습니다.
					nnInfo = graph.GetNearest(position, constraint);
				}

				GraphNode node = nnInfo.node;

				// 이 그래프에서 노드를 찾지 못한 경우
				if (node == null) {
					continue;
				}

				// 요청된 위치에서 노드의 가장 가까운 점까지의 거리
				float dist = ((Vector3)nnInfo.clampedPosition-position).magnitude;

				if (prioritizeGraphs && dist < prioritizeGraphsLimit) {
					// 노드가 충분히 가까우면 이 그래프를 선택하고 다른 것을 모두 무시합니다.
					minDist = dist;
					nearestNode = nnInfo;
					nearestGraph = i;
					break;
				} else {
					// 지금까지 찾은 최적의 노드를 선택합니다.
					if (dist < minDist) {
						minDist = dist;
						nearestNode = nnInfo;
						nearestGraph = i;
					}
				}
			}
		}

		// 일치하는 항목을 찾지 못한 경우
		if (nearestGraph == -1) {
			return new NNInfo();
		}

		// 이미 제약 노드가 설정되었는지 확인합니다.
		if (nearestNode.constrainedNode != null) {
			nearestNode.node = nearestNode.constrainedNode;
			nearestNode.clampedPosition = nearestNode.constClampedPosition;
		}

		if (!fullGetNearestSearch && nearestNode.node != null && !constraint.Suitable(nearestNode.node)) {
			// 그렇지 않으면, 그래프가 적합한 노드를 확인하도록 강제로 검사합니다.
			NNInfoInternal nnInfo = graphs[nearestGraph].GetNearestForce(position, constraint);

			if (nnInfo.node != null) {
				nearestNode = nnInfo;
			}
		}

		if (!constraint.Suitable(nearestNode.node) || (constraint.constrainDistance && (nearestNode.clampedPosition - position).sqrMagnitude > maxNearestNodeDistanceSqr)) {
			return new NNInfo();
		}

		// 내부 필드가 모두 있는 NNInfo로 변환합니다.
		return new NNInfo(nearestNode);
	}

	/// <summary>
	/// 레이에 가장 가까운 노드를 반환합니다 (느립니다).
	/// 경고: 이 함수는 무차별 대입적이며 매우 느릴 수 있으므로 주의해서 사용하세요.
	/// </summary>
	public GraphNode GetNearest (Ray ray) {
		if (graphs == null) return null;

		float minDist = Mathf.Infinity;
		GraphNode nearestNode = null;

		Vector3 lineDirection = ray.direction;
		Vector3 lineOrigin = ray.origin;

		for (int i = 0; i < graphs.Length; i++) {
			NavGraph graph = graphs[i];

			graph.GetNodes(node => {
				Vector3 pos = (Vector3)node.position;
				Vector3 p = lineOrigin+(Vector3.Dot(pos-lineOrigin, lineDirection)*lineDirection);

				float tmp = Mathf.Abs(p.x-pos.x);
				tmp *= tmp;
				if (tmp > minDist) return;

				tmp = Mathf.Abs(p.z-pos.z);
				tmp *= tmp;
				if (tmp > minDist) return;

				float dist = (p-pos).sqrMagnitude;

				if (dist < minDist) {
					minDist = dist;
					nearestNode = node;
				}
				return;
			});
		}

		return nearestNode;
	}
}
