# SpellGameDev — AI 驱动的 Unity 游戏开发工作空间

一个集成了 **通用游戏框架**、**AI 辅助工作流** 和 **示例项目** 的 Unity 游戏开发工作空间。旨在通过标准化的开发流程和可复用的框架模块，大幅提升 Unity 游戏开发效率。

## 核心特性

- **通用游戏框架** — 以 UPM 本地包形式提供 14 个核心模块，开箱即用
- **AI 辅助工作流** — 覆盖需求分析到知识沉淀的 6 阶段完整开发 SOP
- **三层架构规范** — Manager(逻辑) + Data(数据) + View(表现) 分层解耦
- **事件驱动通信** — 全局 EventSystem 实现模块间零耦合

## 目录结构

```
SpellGameDev/
├── Packages/                              # 共用框架 & 第三方 SDK（UPM 本地包）
│   ├── com.gameframework.core/           # GameFramework 核心包
│   │   ├── package.json
│   │   ├── Runtime/
│   │   │   ├── Core/          # Singleton, MonoSingleton, GameManager
│   │   │   ├── Event/         # EventSystem 事件系统
│   │   │   ├── FSM/           # 有限状态机
│   │   │   ├── UI/            # UIManager, UIPanel, UILayer
│   │   │   ├── Audio/         # 音频管理（BGM交叉渐变 + SFX对象池）
│   │   │   ├── Resource/      # 可插拔资源加载（IResourceLoader）
│   │   │   ├── Pool/          # GameObject 对象池
│   │   │   ├── Save/          # 多槽位存档（JSON + 可选加密）
│   │   │   ├── Config/        # JSON 配置加载
│   │   │   ├── Scene/         # 异步场景管理
│   │   │   ├── Input/         # 输入映射（键盘/虚拟轴/触屏）
│   │   │   ├── Platform/      # 跨平台抽象层（存储/文件/音频）
│   │   │   ├── Log/           # 分级日志（编译期控制）
│   │   │   ├── Debug/         # 运行时调试控制台
│   │   │   └── Utils/         # 扩展方法 + 定时器
│   │   └── Editor/
│   │       └── Platform/      # 微信小游戏一键配置工具
│   │
│   └── com.qq.weixin.minigame/           # 微信小游戏 SDK（预下载，按需引用）
│       ├── Editor/            # 转换工具面板
│       ├── Runtime/           # WeChatWASM 运行时 API
│       └── WebGLTemplates/    # WebGL 模板
│
├── Projects/                              # 各游戏项目
│   └── Sudoku/                           # 示例项目：数独游戏
│       ├── Assets/Scripts/
│       │   ├── Data/          # 数据模型
│       │   ├── Logic/         # 核心业务逻辑
│       │   ├── Generator/     # 数独生成算法
│       │   ├── UI/            # 9 个 View 组件
│       │   └── GameFlow/      # 游戏入口
│       └── Packages/manifest.json
│
├── Workflow/                              # AI 工作流模板
│   └── Prompts/
│       ├── 01_需求分析.md
│       ├── 02_编码实现.md
│       ├── 03_测试调试.md
│       ├── 04_重构优化.md
│       ├── 05_内容资源.md
│       └── 06_知识沉淀.md
│
└── .codebuddy/rules/                     # CodeBuddy AI 编码规则
```

## 框架模块一览

| 模块 | 基类 | 核心功能 |
|------|------|----------|
| **Core** | `Singleton<T>` / `MonoSingleton<T>` | 线程安全单例、框架生命周期管理 |
| **EventSystem** | `Singleton` | 发布-订阅模式，支持带参/无参事件 |
| **FSM** | `StateMachine` / `IState` | 通用状态机 + 全局 GameStateManager |
| **UIManager** | `MonoSingleton` | 自动 Canvas 创建、5 层级管理、面板栈 |
| **AudioManager** | `MonoSingleton` | BGM 交叉渐变、SFX 对象池(16路)、3 级音量 |
| **ResourceManager** | `Singleton` | 可插拔加载接口，默认 Resources.Load |
| **PoolManager** | `MonoSingleton` | GameObject 对象池，预热 + 容量限制 |
| **PlatformManager** | `Singleton` | 跨平台抽象层（存储/文件系统/音频），自动检测平台 |
| **SaveManager** | `Singleton` | 5 槽位存档、JSON 序列化、XOR 加密（通过 Platform 层跨平台） |
| **ConfigManager** | `Singleton` | JSON 配置加载 + 缓存 |
| **SceneManager** | `MonoSingleton` | 异步加载/卸载、进度回调 |
| **InputManager** | `MonoSingleton` | 按键映射（主/副键）、虚拟轴、触屏支持 |
| **GameLogger** | 静态类 | 4 级日志、`[Conditional]` 编译控制 |
| **DebugConsole** | `MonoSingleton` | 运行时 IMGUI 控制台、自定义命令注册 |
| **TimerManager** | `MonoSingleton` | 延时/重复/下一帧调用、RealTime 支持 |

## 快速开始

### 1. 创建新项目

```bash
# 在 Projects/ 下新建项目目录
mkdir -p Projects/MyGame/Assets/Scripts/{Data,Logic,UI,GameFlow}
mkdir -p Projects/MyGame/Assets/{Configs,Prefabs,Scenes,Art,Audio,Resources}
mkdir -p Projects/MyGame/Packages
```

### 2. 引用框架包

在 `Projects/MyGame/Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.gameframework.core": "file:../../../Packages/com.gameframework.core"
  }
}
```

### 3. 接入微信小游戏（可选）

Unity 菜单 → **GameFramework → WX MiniGame → One-Click Setup**，一键完成：
- 自动从工作空间引用预下载的微信 SDK（`com.qq.weixin.minigame`）
- 添加 `WEIXINMINIGAME` 编译符号
- 切换到 WebGL 平台
- `PlatformManager` 自动切换到微信实现，业务代码零改动

### 4. 创建游戏入口

```csharp
using UnityEngine;
using GameFramework;

public class MyGameEntry : MonoBehaviour
{
    private void Start()
    {
        // 初始化框架
        GameManager.Instance.Init();

        // 初始化业务 Manager
        // MyGameManager.Instance.Init();
    }
}
```

### 5. 遵循三层架构

```csharp
// Data — 纯数据
[System.Serializable]
public class PlayerData
{
    public int hp;
    public int maxHp;
}

// Manager — 业务逻辑
public class PlayerManager : Singleton<PlayerManager>
{
    private PlayerData _data;

    public void TakeDamage(int damage)
    {
        _data.hp -= damage;
        EventSystem.Instance.Trigger("Player.HpChanged");
    }
}

// View — 表现层
public class PlayerHpView : MonoBehaviour
{
    private void OnEnable()
    {
        EventSystem.Instance.Subscribe("Player.HpChanged", OnHpChanged);
    }

    private void OnHpChanged()
    {
        // 从 Manager 读取数据刷新 UI
    }
}
```

## AI 工作流（6 阶段）

本工作空间配套了一套完整的 AI 辅助开发工作流，通过 Prompt 模板和 CodeBuddy 规则文件驱动：

| 阶段 | 触发时机 | 核心产出 |
|------|----------|----------|
| **01 需求分析** | 新功能/需求变更 | 技术设计文档 (TDD) |
| **02 编码实现** | 设计确认后 | Data → Logic → View 代码 |
| **03 测试调试** | 功能完成/Bug 报告 | 定位方案 + 测试用例 |
| **04 重构优化** | 代码臃肿/性能问题 | 诊断报告 + 重构方案 |
| **05 内容资源** | 数值/关卡/PCG | 公式 + 算法 + 资源规格 |
| **06 知识沉淀** | 功能完成后 | 架构决策记录 + 总结 |

## 跨平台抽象层（Platform）

框架内置了跨平台抽象层，使业务代码无需修改即可在不同平台运行：

| 接口 | 职责 | Default 实现 | WX 实现 |
|------|------|-------------|---------|
| `IPlatformStorage` | 键值对存储 | PlayerPrefs | WX.StorageSetSync |
| `IPlatformFileSystem` | 文件读写 | System.IO | WX.FileSystemManager |
| `IPlatformAudio` | 音频播放 | Unity AudioSource | WX.InnerAudioContext |

**PlatformManager** 在 `GameManager` 初始化序列中自动根据编译符号选择实现，也支持通过 `SetStorage()`/`SetFileSystem()`/`SetAudio()` 手动注入自定义实现。

### 微信小游戏接入流程

1. Unity 菜单 → **GameFramework → WX MiniGame → One-Click Setup**
2. 脚本自动：导入 SDK → 添加 `WEIXINMINIGAME` 编译符号 → 切换 WebGL
3. `PlatformManager` 自动激活微信实现，`SaveManager` 等上层模块无感切换
4. 开发阶段切回：**Remove Define Symbol** + **Switch to Standalone**

### 微信小游戏构建发布

1. Unity 菜单 → **GameFramework → WX MiniGame → Build MiniGame (Full)**（或 Dev 模式）
2. 首次构建时若导出路径未配置，会弹出对话框：
   - **使用默认** → 导出到项目根目录下的 `WXExport/`
   - **自定义** → 选择其他目录
   - 路径自动持久化到 SDK 配置，后续构建不再弹窗
3. 构建完成后，产物在导出目录的 `minigame/` 子目录
4. 用**微信开发者工具**打开 `minigame/` 目录即可预览/上传

## 示例项目：数独

`Projects/Sudoku/` 是一个完整的数独游戏实现，展示了框架的使用方式：

- **4 级难度**（Easy / Medium / Hard / Expert）
- **回溯法生成器** — 保证唯一解
- **笔记系统** — 候选数标记 + 自动清除关联笔记
- **撤销/重做** — 命令模式双栈实现
- **提示系统** — 每局 3 次提示机会
- **冲突检测** — 实时行/列/宫重复检查 + 高亮
- **存档/续玩** — 中途退出自动保存
- **统计系统** — 各难度胜率/最佳用时/连胜记录

## 环境要求

- **Unity** 2021.3+
- **C#** 9.0+
- **IDE** 推荐配合 CodeBuddy 使用 AI 工作流

## 编码规范

| 类别 | 规则 |
|------|------|
| 类/方法 | PascalCase |
| 私有字段 | _camelCase（下划线前缀） |
| 常量 | UPPER_SNAKE_CASE |
| 接口 | I + PascalCase |
| 单文件上限 | 300 行 |
| 单方法上限 | 40 行 |
| Public API | 必须有 XML 注释 |
| 跨模块通信 | 仅通过 EventSystem |

## License

私有项目，仅供内部使用。
