#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sudoku.GameFlow;
using Sudoku.UI;

namespace Sudoku.Editor
{
    /// <summary>
    /// 数独场景一键搭建工具。
    /// 通过菜单 Sudoku/Build Scene 自动生成完整的数独游戏场景。
    /// </summary>
    public static class SudokuSceneBuilder
    {
        // ============ 颜色常量 ============
        private static readonly Color COLOR_BG = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color COLOR_PRIMARY = new Color(0.2f, 0.4f, 0.8f, 1f);
        private static readonly Color COLOR_PRIMARY_DARK = new Color(0.15f, 0.3f, 0.65f, 1f);
        private static readonly Color COLOR_ACCENT = new Color(0.9f, 0.55f, 0.1f, 1f);
        private static readonly Color COLOR_TEXT_DARK = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color COLOR_TEXT_LIGHT = Color.white;
        private static readonly Color COLOR_PANEL_BG = new Color(1f, 1f, 1f, 0.97f);
        private static readonly Color COLOR_OVERLAY = new Color(0f, 0f, 0f, 0.5f);
        private static readonly Color COLOR_CELL_NORMAL = Color.white;
        private static readonly Color COLOR_CELL_FIXED = new Color(0.92f, 0.92f, 0.92f, 1f);
        private static readonly Color COLOR_CELL_SELECTED = new Color(0.78f, 0.88f, 1f, 1f);
        private static readonly Color COLOR_CELL_HIGHLIGHT = new Color(0.9f, 0.94f, 1f, 1f);
        private static readonly Color COLOR_CELL_ERROR = new Color(1f, 0.85f, 0.85f, 1f);
        private static readonly Color COLOR_NOTE_INACTIVE = new Color(0.6f, 0.6f, 0.6f, 1f);
        private static readonly Color COLOR_BUTTON_FUNC = new Color(0.93f, 0.93f, 0.93f, 1f);

        // ============ 尺寸常量 ============
        private const float CANVAS_REF_WIDTH = 1080f;
        private const float CANVAS_REF_HEIGHT = 1920f;
        private const float CELL_SIZE = 95f;
        private const float CELL_SPACING = 2f;
        private const float BOARD_SIZE = CELL_SIZE * 9 + CELL_SPACING * 8;
        private const int FONT_SIZE_TITLE = 56;
        private const int FONT_SIZE_LARGE = 44;
        private const int FONT_SIZE_NORMAL = 36;
        private const int FONT_SIZE_SMALL = 28;
        private const int FONT_SIZE_CELL = 44;
        private const int FONT_SIZE_NOTE = 16;

        [MenuItem("Sudoku/Build Scene", false, 1)]
        public static void BuildScene()
        {
            if (!EditorUtility.DisplayDialog(
                "数独场景搭建",
                "将在当前场景中创建完整的数独游戏 UI 结构。\n\n注意：会清空当前场景中的所有对象。\n\n继续？",
                "确认搭建", "取消"))
            {
                return;
            }

            // 清空场景
            ClearScene();

            // 创建基础设施
            CreateCamera();
            CreateEventSystem();
            var gameEntry = CreateGameEntry();

            // 创建 Canvas
            var canvas = CreateMainCanvas();
            var canvasRT = canvas.GetComponent<RectTransform>();

            // 创建各面板
            var menuPanel = CreateMenuPanel(canvasRT);
            var gamePanel = CreateGamePanel(canvasRT);
            var pausePanel = CreatePausePanel(canvasRT);
            var completePanel = CreateGameCompletePanel(canvasRT);
            var statsPanel = CreateStatsPanel(canvasRT);

            // 创建 Cell 预制体
            var cellPrefab = CreateCellPrefab();

            // ============ 绑定组件引用 ============
            BindBoardView(gamePanel, cellPrefab);
            BindNumberPadView(gamePanel);
            BindTimerView(gamePanel);
            BindGameInfoView(gamePanel);
            BindMainMenuView(menuPanel, gamePanel, statsPanel);
            BindPauseView(gamePanel, pausePanel, menuPanel);
            BindGameCompleteView(completePanel, gamePanel, menuPanel);
            BindStatsView(statsPanel, menuPanel);

            // 初始状态
            menuPanel.SetActive(true);
            gamePanel.SetActive(false);
            pausePanel.SetActive(false);
            completePanel.SetActive(false);
            statsPanel.SetActive(false);

            // 保存场景
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("完成", "数独场景搭建完成！\n\n请保存场景（Ctrl+S）。", "好的");
        }

        [MenuItem("Sudoku/Create Cell Prefab Only", false, 2)]
        public static void CreateCellPrefabOnly()
        {
            CreateCellPrefab();
            EditorUtility.DisplayDialog("完成", "CellPrefab 已生成到 Assets/Prefabs/Board/CellPrefab.prefab", "好的");
        }

        // ======================================================================
        //  场景基础设施
        // ======================================================================

        private static void ClearScene()
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var go in roots)
            {
                Object.DestroyImmediate(go);
            }
        }

        private static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera");
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = COLOR_BG;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            go.AddComponent<AudioListener>();
            go.tag = "MainCamera";
            return cam;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateGameEntry()
        {
            var go = new GameObject("[SudokuGameEntry]");
            go.AddComponent<SudokuGameEntry>();
            return go;
        }

        // ======================================================================
        //  Canvas
        // ======================================================================

        private static Canvas CreateMainCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(CANVAS_REF_WIDTH, CANVAS_REF_HEIGHT);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ======================================================================
        //  主菜单面板
        // ======================================================================

        private static GameObject CreateMenuPanel(RectTransform parent)
        {
            var panel = CreateFullScreenPanel(parent, "MenuPanel", COLOR_PANEL_BG);
            var panelRT = panel.GetComponent<RectTransform>();

            // 标题
            var title = CreateText(panelRT, "Title", "数 独", FONT_SIZE_TITLE, COLOR_PRIMARY);
            SetAnchors(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            title.anchoredPosition = new Vector2(0, -200);
            title.sizeDelta = new Vector2(600, 80);

            // 按钮容器
            var btnContainer = CreateUIObject(panelRT, "ButtonContainer");
            SetAnchors(btnContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            btnContainer.sizeDelta = new Vector2(500, 600);
            btnContainer.anchoredPosition = new Vector2(0, 50);
            var vlg = btnContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 24;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // 继续游戏按钮
            CreateMenuButton(btnContainer, "ContinueButton", "继续游戏", COLOR_ACCENT, 80);

            // 难度按钮
            CreateMenuButton(btnContainer, "EasyButton", "简    单", COLOR_PRIMARY, 80);
            CreateMenuButton(btnContainer, "MediumButton", "中    等", COLOR_PRIMARY, 80);
            CreateMenuButton(btnContainer, "HardButton", "困    难", COLOR_PRIMARY, 80);
            CreateMenuButton(btnContainer, "ExpertButton", "专    家", COLOR_PRIMARY_DARK, 80);

            // 统计按钮
            var statsBtn = CreateMenuButton(btnContainer, "StatsButton", "统    计", COLOR_BUTTON_FUNC, 80);
            statsBtn.transform.Find("Text").GetComponent<Text>().color = COLOR_TEXT_DARK;

            return panel;
        }

        // ======================================================================
        //  游戏面板
        // ======================================================================

        private static GameObject CreateGamePanel(RectTransform parent)
        {
            var panel = CreateFullScreenPanel(parent, "GamePanel", COLOR_BG);
            var panelRT = panel.GetComponent<RectTransform>();

            // === 顶部信息栏 (顶部 3% ~ 6%) ===
            var topBar = CreateUIObject(panelRT, "TopBar");
            topBar.anchorMin = new Vector2(0, 0.94f);
            topBar.anchorMax = new Vector2(1, 0.97f);
            topBar.offsetMin = new Vector2(40, 0);
            topBar.offsetMax = new Vector2(-40, 0);
            var topHLG = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            topHLG.spacing = 20;
            topHLG.childAlignment = TextAnchor.MiddleCenter;
            topHLG.childForceExpandWidth = true;
            topHLG.childForceExpandHeight = true;
            topHLG.childControlWidth = true;
            topHLG.childControlHeight = true;
            topBar.gameObject.AddComponent<GameInfoView>();
            CreateText(topBar, "DifficultyText", "中等", FONT_SIZE_SMALL, COLOR_TEXT_DARK);
            CreateText(topBar, "ErrorsText", "错误: 0", FONT_SIZE_SMALL, COLOR_TEXT_DARK);
            CreateText(topBar, "HintsText", "提示: 3", FONT_SIZE_SMALL, COLOR_TEXT_DARK);

            // === 计时器行 (顶部 6% ~ 9%) ===
            var timerRow = CreateUIObject(panelRT, "TimerRow");
            timerRow.anchorMin = new Vector2(0, 0.91f);
            timerRow.anchorMax = new Vector2(1, 0.94f);
            timerRow.offsetMin = new Vector2(40, 0);
            timerRow.offsetMax = new Vector2(-40, 0);
            var timerHLG = timerRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            timerHLG.spacing = 20;
            timerHLG.childAlignment = TextAnchor.MiddleCenter;
            timerHLG.childForceExpandWidth = true;
            timerHLG.childForceExpandHeight = true;

            // 左占位
            var leftSpacer = CreateUIObject(timerRow, "LeftSpacer");
            leftSpacer.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            // TimerView
            var timerObj = CreateUIObject(timerRow, "TimerView");
            timerObj.gameObject.AddComponent<TimerView>();
            var timerViewLE = timerObj.gameObject.AddComponent<LayoutElement>();
            timerViewLE.preferredWidth = 200;
            timerViewLE.flexibleWidth = 0;
            CreateText(timerObj, "TimerText", "00:00", FONT_SIZE_NORMAL, COLOR_TEXT_DARK);

            // 暂停按钮
            var pauseBtnRT = CreateButtonObject(timerRow, "PauseButton", "||", FONT_SIZE_NORMAL, COLOR_BUTTON_FUNC, COLOR_TEXT_DARK, 0);
            var pauseLE = pauseBtnRT.gameObject.AddComponent<LayoutElement>();
            pauseLE.preferredWidth = 80;
            pauseLE.flexibleWidth = 0;

            // === 棋盘区域 (居中，锚点 center，固定尺寸) ===
            var boardArea = CreateUIObject(panelRT, "BoardArea");
            SetAnchors(boardArea, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            float boardAreaSize = BOARD_SIZE + 20;
            boardArea.sizeDelta = new Vector2(boardAreaSize, boardAreaSize);
            boardArea.anchoredPosition = new Vector2(0, 80);
            var boardBG = boardArea.gameObject.AddComponent<Image>();
            boardBG.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // 棋盘格子容器
            var boardRoot = CreateUIObject(boardArea, "BoardRoot");
            SetAnchors(boardRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            boardRoot.sizeDelta = new Vector2(BOARD_SIZE, BOARD_SIZE);
            boardRoot.anchoredPosition = Vector2.zero;
            var grid = boardRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CELL_SIZE, CELL_SIZE);
            grid.spacing = new Vector2(CELL_SPACING, CELL_SPACING);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 9;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;

            boardArea.gameObject.AddComponent<BoardView>();

            // 宫格分隔线
            CreateBlockSeparators(boardArea, BOARD_SIZE);

            // === 数字键盘 (底部区域) ===
            var numPadArea = CreateUIObject(panelRT, "NumberPad");
            numPadArea.anchorMin = new Vector2(0, 0.02f);
            numPadArea.anchorMax = new Vector2(1, 0.18f);
            numPadArea.offsetMin = new Vector2(40, 0);
            numPadArea.offsetMax = new Vector2(-40, 0);
            numPadArea.gameObject.AddComponent<NumberPadView>();

            var numPadVLG = numPadArea.gameObject.AddComponent<VerticalLayoutGroup>();
            numPadVLG.spacing = 12;
            numPadVLG.childAlignment = TextAnchor.MiddleCenter;
            numPadVLG.childForceExpandWidth = true;
            numPadVLG.childForceExpandHeight = false;
            numPadVLG.childControlWidth = true;
            numPadVLG.childControlHeight = false;

            // 数字按钮行 (1-9)
            var numRow = CreateUIObject(numPadArea.GetComponent<RectTransform>(), "NumberRow");
            var numRowLE = numRow.gameObject.AddComponent<LayoutElement>();
            numRowLE.preferredHeight = 100;
            numRowLE.flexibleHeight = 1;
            var numRowHLG = numRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            numRowHLG.spacing = 6;
            numRowHLG.childAlignment = TextAnchor.MiddleCenter;
            numRowHLG.childForceExpandWidth = true;
            numRowHLG.childForceExpandHeight = true;
            numRowHLG.childControlWidth = true;
            numRowHLG.childControlHeight = true;

            for (int i = 1; i <= 9; i++)
            {
                CreateButtonObject(numRow, $"NumberButton_{i}", i.ToString(), FONT_SIZE_LARGE, COLOR_PANEL_BG, COLOR_PRIMARY, 0);
            }

            // 功能按钮行
            var funcRow = CreateUIObject(numPadArea.GetComponent<RectTransform>(), "FuncRow");
            var funcRowLE = funcRow.gameObject.AddComponent<LayoutElement>();
            funcRowLE.preferredHeight = 80;
            funcRowLE.flexibleHeight = 1;
            var funcRowHLG = funcRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            funcRowHLG.spacing = 12;
            funcRowHLG.childAlignment = TextAnchor.MiddleCenter;
            funcRowHLG.childForceExpandWidth = true;
            funcRowHLG.childForceExpandHeight = true;
            funcRowHLG.childControlWidth = true;
            funcRowHLG.childControlHeight = true;

            CreateFuncButton(funcRow, "UndoButton", "撤销");
            CreateFuncButton(funcRow, "RedoButton", "重做");
            CreateFuncButton(funcRow, "EraseButton", "擦除");
            var noteBtn = CreateFuncButton(funcRow, "NoteButton", "笔记");
            var noteIndicator = CreateUIObject(noteBtn.GetComponent<RectTransform>(), "NoteIndicator");
            SetAnchors(noteIndicator, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            noteIndicator.sizeDelta = new Vector2(30, 6);
            noteIndicator.anchoredPosition = new Vector2(0, -4);
            var noteIndImg = noteIndicator.gameObject.AddComponent<Image>();
            noteIndImg.color = COLOR_NOTE_INACTIVE;
            CreateFuncButton(funcRow, "HintButton", "提示");

            // PauseView 挂在 GamePanel 上
            panelRT.gameObject.AddComponent<PauseView>();

            return panel;
        }

        // ======================================================================
        //  暂停面板
        // ======================================================================

        private static GameObject CreatePausePanel(RectTransform parent)
        {
            var root = CreateUIObject(parent, "PausePanel");
            SetFullStretch(root);

            // 遮罩
            var overlay = CreateFullScreenPanel(parent, "PauseOverlay_temp", COLOR_OVERLAY);
            overlay.transform.SetParent(root, false);
            var overlayRT = overlay.GetComponent<RectTransform>();
            SetFullStretch(overlayRT);
            Object.DestroyImmediate(overlay.GetComponent<Image>());
            var overlayImg = root.gameObject.AddComponent<Image>();
            overlayImg.color = COLOR_OVERLAY;

            // 弹窗
            var panel = CreateUIObject(root, "Panel");
            SetAnchors(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            panel.sizeDelta = new Vector2(500, 380);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = COLOR_PANEL_BG;

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 24;
            vlg.padding = new RectOffset(40, 40, 40, 40);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // 标题
            var title = CreateText(panel, "Title", "游 戏 暂 停", FONT_SIZE_LARGE, COLOR_TEXT_DARK);
            var titleLE = title.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 60;

            CreateMenuButton(panel, "ResumeButton", "继续游戏", COLOR_PRIMARY, 70);
            CreateMenuButton(panel, "MenuButton", "返回主菜单", COLOR_BUTTON_FUNC, 70);
            panel.transform.Find("MenuButton/Text").GetComponent<Text>().color = COLOR_TEXT_DARK;

            // 清理临时对象
            Object.DestroyImmediate(overlay);

            return root.gameObject;
        }

        // ======================================================================
        //  游戏完成面板
        // ======================================================================

        private static GameObject CreateGameCompletePanel(RectTransform parent)
        {
            var root = CreateUIObject(parent, "GameCompletePanel");
            SetFullStretch(root);
            var rootImg = root.gameObject.AddComponent<Image>();
            rootImg.color = COLOR_OVERLAY;

            var panel = CreateUIObject(root, "Panel");
            SetAnchors(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            panel.sizeDelta = new Vector2(550, 550);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = COLOR_PANEL_BG;

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(40, 40, 40, 40);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var titleRT = CreateText(panel, "TitleText", "恭喜完成！", FONT_SIZE_TITLE, COLOR_PRIMARY);
            titleRT.gameObject.AddComponent<LayoutElement>().preferredHeight = 70;

            var diffRT = CreateText(panel, "DifficultyText", "难度: 中等", FONT_SIZE_NORMAL, COLOR_TEXT_DARK);
            diffRT.gameObject.AddComponent<LayoutElement>().preferredHeight = 50;

            var timeRT = CreateText(panel, "TimeText", "用时: 00:00", FONT_SIZE_NORMAL, COLOR_TEXT_DARK);
            timeRT.gameObject.AddComponent<LayoutElement>().preferredHeight = 50;

            var bestRT = CreateText(panel, "BestTimeText", "最佳时间: 00:00", FONT_SIZE_SMALL, COLOR_ACCENT);
            bestRT.gameObject.AddComponent<LayoutElement>().preferredHeight = 45;

            // 按钮
            CreateMenuButton(panel, "NewGameButton", "再来一局", COLOR_PRIMARY, 70);
            CreateMenuButton(panel, "MenuButton", "返回主菜单", COLOR_BUTTON_FUNC, 70);
            panel.transform.Find("MenuButton/Text").GetComponent<Text>().color = COLOR_TEXT_DARK;

            root.gameObject.AddComponent<GameCompleteView>();
            return root.gameObject;
        }

        // ======================================================================
        //  统计面板
        // ======================================================================

        private static GameObject CreateStatsPanel(RectTransform parent)
        {
            var panel = CreateFullScreenPanel(parent, "StatsPanel", COLOR_PANEL_BG);
            var panelRT = panel.GetComponent<RectTransform>();
            panel.AddComponent<StatsView>();

            // 标题
            var titleRT = CreateText(panelRT, "Title", "游 戏 统 计", FONT_SIZE_TITLE, COLOR_PRIMARY);
            SetAnchors(titleRT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            titleRT.anchoredPosition = new Vector2(0, -120);
            titleRT.sizeDelta = new Vector2(600, 70);

            // 统计内容容器
            var container = CreateUIObject(panelRT, "StatsContainer");
            SetAnchors(container, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            container.sizeDelta = new Vector2(800, 600);
            container.anchoredPosition = new Vector2(0, 30);
            var vlg = container.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            CreateStatsRow(container, "EasyStatsText", "简单  |  已玩: 0  胜: 0  胜率: 0%  最佳: --:--");
            CreateStatsRow(container, "MediumStatsText", "中等  |  已玩: 0  胜: 0  胜率: 0%  最佳: --:--");
            CreateStatsRow(container, "HardStatsText", "困难  |  已玩: 0  胜: 0  胜率: 0%  最佳: --:--");
            CreateStatsRow(container, "ExpertStatsText", "专家  |  已玩: 0  胜: 0  胜率: 0%  最佳: --:--");

            // 分隔
            var sep = CreateUIObject(container.GetComponent<RectTransform>(), "Separator");
            sep.gameObject.AddComponent<LayoutElement>().preferredHeight = 10;

            CreateStatsRow(container, "StreakText", "当前连胜: 0  |  最高连胜: 0");

            // 返回按钮
            var backBtn = CreateMenuButton(container.GetComponent<RectTransform>(), "BackButton", "返    回", COLOR_PRIMARY, 70);

            return panel;
        }

        // ======================================================================
        //  Cell 预制体创建
        // ======================================================================

        private static GameObject CreateCellPrefab()
        {
            var cell = new GameObject("CellPrefab");
            var cellRT = cell.AddComponent<RectTransform>();
            cellRT.sizeDelta = new Vector2(CELL_SIZE, CELL_SIZE);

            // Background (Image + Button)
            var bg = cell.AddComponent<Image>();
            bg.color = COLOR_CELL_NORMAL;
            var btn = cell.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = COLOR_CELL_HIGHLIGHT;
            colors.pressedColor = COLOR_CELL_SELECTED;
            btn.colors = colors;

            // 数字文本
            var valueGO = new GameObject("ValueText");
            valueGO.transform.SetParent(cell.transform, false);
            var valueRT = valueGO.AddComponent<RectTransform>();
            SetFullStretch(valueRT);
            var valueText = valueGO.AddComponent<Text>();
            valueText.text = "";
            valueText.fontSize = FONT_SIZE_CELL;
            valueText.alignment = TextAnchor.MiddleCenter;
            valueText.color = COLOR_TEXT_DARK;
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.raycastTarget = false;

            // 笔记容器
            var notesContainer = new GameObject("NotesContainer");
            notesContainer.transform.SetParent(cell.transform, false);
            var notesRT = notesContainer.AddComponent<RectTransform>();
            SetFullStretch(notesRT);
            notesRT.offsetMin = new Vector2(4, 4);
            notesRT.offsetMax = new Vector2(-4, -4);
            var notesGrid = notesContainer.AddComponent<GridLayoutGroup>();
            float noteCellSize = (CELL_SIZE - 12) / 3f;
            notesGrid.cellSize = new Vector2(noteCellSize, noteCellSize);
            notesGrid.spacing = Vector2.zero;
            notesGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            notesGrid.constraintCount = 3;
            notesGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            notesGrid.childAlignment = TextAnchor.MiddleCenter;

            // 9 个笔记文本
            for (int i = 0; i < 9; i++)
            {
                var noteGO = new GameObject($"NoteText_{i}");
                noteGO.transform.SetParent(notesContainer.transform, false);
                var noteText = noteGO.AddComponent<Text>();
                noteText.text = "";
                noteText.fontSize = FONT_SIZE_NOTE;
                noteText.alignment = TextAnchor.MiddleCenter;
                noteText.color = COLOR_PRIMARY;
                noteText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                noteText.raycastTarget = false;
            }

            // CellView 组件 + 绑定引用
            var cellView = cell.AddComponent<CellView>();
            var cellSO = new SerializedObject(cellView);
            cellSO.FindProperty("_valueText").objectReferenceValue = valueText;
            cellSO.FindProperty("_background").objectReferenceValue = bg;
            cellSO.FindProperty("_button").objectReferenceValue = btn;

            var notesProp = cellSO.FindProperty("_noteTexts");
            notesProp.arraySize = 9;
            for (int i = 0; i < 9; i++)
            {
                notesProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    notesContainer.transform.GetChild(i).GetComponent<Text>();
            }
            cellSO.ApplyModifiedPropertiesWithoutUndo();

            // 保存为预制体
            string prefabDir = "Assets/Prefabs/Board";
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder(prefabDir))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Board");

            string prefabPath = $"{prefabDir}/CellPrefab.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(cell, prefabPath);
            Object.DestroyImmediate(cell);

            UnityEngine.Debug.Log($"[SudokuSceneBuilder] CellPrefab saved to {prefabPath}");
            return prefab;
        }

        // ======================================================================
        //  绑定 SerializeField 引用
        // ======================================================================

        private static void BindBoardView(GameObject gamePanel, GameObject cellPrefab)
        {
            var boardView = gamePanel.transform.Find("BoardArea").GetComponent<BoardView>();
            var boardRoot = gamePanel.transform.Find("BoardArea/BoardRoot");
            var gridLayout = boardRoot.GetComponent<GridLayoutGroup>();

            var so = new SerializedObject(boardView);
            so.FindProperty("_cellPrefab").objectReferenceValue = cellPrefab;
            so.FindProperty("_boardRoot").objectReferenceValue = boardRoot;
            so.FindProperty("_gridLayout").objectReferenceValue = gridLayout;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindNumberPadView(GameObject gamePanel)
        {
            var numPad = gamePanel.transform.Find("NumberPad").GetComponent<NumberPadView>();
            var so = new SerializedObject(numPad);

            // 数字按钮数组
            var numBtnsProp = so.FindProperty("_numberButtons");
            numBtnsProp.arraySize = 9;
            for (int i = 0; i < 9; i++)
            {
                var btn = gamePanel.transform.Find($"NumberPad/NumberRow/NumberButton_{i + 1}");
                numBtnsProp.GetArrayElementAtIndex(i).objectReferenceValue = btn.GetComponent<Button>();
            }

            // 功能按钮
            so.FindProperty("_eraseButton").objectReferenceValue = FindButton(gamePanel, "NumberPad/FuncRow/EraseButton");
            so.FindProperty("_noteButton").objectReferenceValue = FindButton(gamePanel, "NumberPad/FuncRow/NoteButton");
            so.FindProperty("_hintButton").objectReferenceValue = FindButton(gamePanel, "NumberPad/FuncRow/HintButton");
            so.FindProperty("_undoButton").objectReferenceValue = FindButton(gamePanel, "NumberPad/FuncRow/UndoButton");
            so.FindProperty("_redoButton").objectReferenceValue = FindButton(gamePanel, "NumberPad/FuncRow/RedoButton");
            so.FindProperty("_noteIndicator").objectReferenceValue =
                gamePanel.transform.Find("NumberPad/FuncRow/NoteButton/NoteIndicator").GetComponent<Image>();

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindTimerView(GameObject gamePanel)
        {
            var timerView = gamePanel.transform.Find("TimerRow/TimerView").GetComponent<TimerView>();
            var so = new SerializedObject(timerView);
            so.FindProperty("_timerText").objectReferenceValue =
                gamePanel.transform.Find("TimerRow/TimerView/TimerText").GetComponent<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindGameInfoView(GameObject gamePanel)
        {
            var infoView = gamePanel.transform.Find("TopBar").GetComponent<GameInfoView>();
            var so = new SerializedObject(infoView);
            so.FindProperty("_difficultyText").objectReferenceValue =
                gamePanel.transform.Find("TopBar/DifficultyText").GetComponent<Text>();
            so.FindProperty("_errorsText").objectReferenceValue =
                gamePanel.transform.Find("TopBar/ErrorsText").GetComponent<Text>();
            so.FindProperty("_hintsText").objectReferenceValue =
                gamePanel.transform.Find("TopBar/HintsText").GetComponent<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindMainMenuView(GameObject menuPanel, GameObject gamePanel, GameObject statsPanel)
        {
            var menuView = menuPanel.GetComponent<MainMenuView>();
            if (menuView == null)
                menuView = menuPanel.AddComponent<MainMenuView>();

            var so = new SerializedObject(menuView);
            so.FindProperty("_easyButton").objectReferenceValue = FindButton(menuPanel, "ButtonContainer/EasyButton");
            so.FindProperty("_mediumButton").objectReferenceValue = FindButton(menuPanel, "ButtonContainer/MediumButton");
            so.FindProperty("_hardButton").objectReferenceValue = FindButton(menuPanel, "ButtonContainer/HardButton");
            so.FindProperty("_expertButton").objectReferenceValue = FindButton(menuPanel, "ButtonContainer/ExpertButton");
            so.FindProperty("_continueButton").objectReferenceValue = FindButton(menuPanel, "ButtonContainer/ContinueButton");
            so.FindProperty("_statsButton").objectReferenceValue = FindButton(menuPanel, "ButtonContainer/StatsButton");
            so.FindProperty("_menuPanel").objectReferenceValue = menuPanel;
            so.FindProperty("_gamePanel").objectReferenceValue = gamePanel;
            so.FindProperty("_statsPanel").objectReferenceValue = statsPanel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindPauseView(GameObject gamePanel, GameObject pausePanel, GameObject menuPanel)
        {
            var pauseView = gamePanel.GetComponent<PauseView>();
            var menuView = menuPanel.GetComponent<MainMenuView>();

            var so = new SerializedObject(pauseView);
            so.FindProperty("_panel").objectReferenceValue = pausePanel.transform.Find("Panel").gameObject;
            so.FindProperty("_pauseButton").objectReferenceValue =
                gamePanel.transform.Find("TimerRow/PauseButton").GetComponent<Button>();
            so.FindProperty("_resumeButton").objectReferenceValue =
                FindButton(pausePanel, "Panel/ResumeButton");
            so.FindProperty("_menuButton").objectReferenceValue =
                FindButton(pausePanel, "Panel/MenuButton");
            so.FindProperty("_gamePanel").objectReferenceValue = gamePanel;
            so.FindProperty("_mainMenuView").objectReferenceValue = menuView;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindGameCompleteView(GameObject completePanel, GameObject gamePanel, GameObject menuPanel)
        {
            var completeView = completePanel.GetComponent<GameCompleteView>();
            var menuView = menuPanel.GetComponent<MainMenuView>();
            var panel = completePanel.transform.Find("Panel");

            var so = new SerializedObject(completeView);
            so.FindProperty("_panel").objectReferenceValue = panel.gameObject;
            so.FindProperty("_titleText").objectReferenceValue = panel.Find("TitleText").GetComponent<Text>();
            so.FindProperty("_timeText").objectReferenceValue = panel.Find("TimeText").GetComponent<Text>();
            so.FindProperty("_difficultyText").objectReferenceValue = panel.Find("DifficultyText").GetComponent<Text>();
            so.FindProperty("_bestTimeText").objectReferenceValue = panel.Find("BestTimeText").GetComponent<Text>();
            so.FindProperty("_newGameButton").objectReferenceValue = panel.Find("NewGameButton").GetComponent<Button>();
            so.FindProperty("_menuButton").objectReferenceValue = panel.Find("MenuButton").GetComponent<Button>();
            so.FindProperty("_gamePanel").objectReferenceValue = gamePanel;
            so.FindProperty("_mainMenuView").objectReferenceValue = menuView;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindStatsView(GameObject statsPanel, GameObject menuPanel)
        {
            var statsView = statsPanel.GetComponent<StatsView>();
            var menuView = menuPanel.GetComponent<MainMenuView>();

            var so = new SerializedObject(statsView);
            so.FindProperty("_panel").objectReferenceValue = statsPanel;
            so.FindProperty("_backButton").objectReferenceValue =
                statsPanel.transform.Find("StatsContainer/BackButton").GetComponent<Button>();
            so.FindProperty("_mainMenuView").objectReferenceValue = menuView;
            so.FindProperty("_easyStatsText").objectReferenceValue =
                statsPanel.transform.Find("StatsContainer/EasyStatsText").GetComponent<Text>();
            so.FindProperty("_mediumStatsText").objectReferenceValue =
                statsPanel.transform.Find("StatsContainer/MediumStatsText").GetComponent<Text>();
            so.FindProperty("_hardStatsText").objectReferenceValue =
                statsPanel.transform.Find("StatsContainer/HardStatsText").GetComponent<Text>();
            so.FindProperty("_expertStatsText").objectReferenceValue =
                statsPanel.transform.Find("StatsContainer/ExpertStatsText").GetComponent<Text>();
            so.FindProperty("_streakText").objectReferenceValue =
                statsPanel.transform.Find("StatsContainer/StreakText").GetComponent<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ======================================================================
        //  宫格粗线分隔
        // ======================================================================

        private static void CreateBlockSeparators(RectTransform boardArea, float boardSize)
        {
            float offset = boardSize / 2f;
            float thickness = 4f;
            Color lineColor = new Color(0.2f, 0.2f, 0.2f, 1f);

            // 垂直线 (2条)
            for (int i = 1; i <= 2; i++)
            {
                float x = -offset + (boardSize / 3f) * i;
                var line = CreateUIObject(boardArea, $"VLine_{i}");
                SetAnchors(line, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                line.sizeDelta = new Vector2(thickness, boardSize + 10);
                line.anchoredPosition = new Vector2(x, 0);
                var img = line.gameObject.AddComponent<Image>();
                img.color = lineColor;
                img.raycastTarget = false;
            }

            // 水平线 (2条)
            for (int i = 1; i <= 2; i++)
            {
                float y = offset - (boardSize / 3f) * i;
                var line = CreateUIObject(boardArea, $"HLine_{i}");
                SetAnchors(line, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                line.sizeDelta = new Vector2(boardSize + 10, thickness);
                line.anchoredPosition = new Vector2(0, y);
                var img = line.gameObject.AddComponent<Image>();
                img.color = lineColor;
                img.raycastTarget = false;
            }
        }

        // ======================================================================
        //  UI 工具方法
        // ======================================================================

        private static RectTransform CreateUIObject(RectTransform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            return rt;
        }

        private static GameObject CreateFullScreenPanel(RectTransform parent, string name, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            SetFullStretch(rt);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            return go;
        }

        private static RectTransform CreateText(RectTransform parent, string name, string text, int fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return rt;
        }

        private static RectTransform CreateButtonObject(RectTransform parent, string name, string label,
            int fontSize, Color bgColor, Color textColor, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            if (height > 0)
                rt.sizeDelta = new Vector2(0, height);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r * 0.9f, bgColor.g * 0.9f, bgColor.b * 0.9f, 1f);
            colors.pressedColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, 1f);
            btn.colors = colors;

            var textRT = CreateText(rt, "Text", label, fontSize, textColor);
            SetFullStretch(textRT);

            return rt;
        }

        private static RectTransform CreateMenuButton(RectTransform parent, string name, string label, Color bgColor, float height)
        {
            var rt = CreateButtonObject(parent, name, label, FONT_SIZE_NORMAL, bgColor, COLOR_TEXT_LIGHT, height);
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return rt;
        }

        private static RectTransform CreateFuncButton(RectTransform parent, string name, string label)
        {
            return CreateButtonObject(parent.GetComponent<RectTransform>(), name, label,
                FONT_SIZE_SMALL, COLOR_BUTTON_FUNC, COLOR_TEXT_DARK, 0);
        }

        private static void CreateStatsRow(RectTransform parent, string name, string defaultText)
        {
            var rt = CreateText(parent.GetComponent<RectTransform>(), name, defaultText, FONT_SIZE_SMALL, COLOR_TEXT_DARK);
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = 50;
        }

        private static Button FindButton(GameObject root, string path)
        {
            var t = root.transform.Find(path);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private static void SetAnchors(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void SetFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
#endif
