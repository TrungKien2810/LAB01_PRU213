# HANDOFF — context / nguồn sự thật

> File này là điểm "tiếp tục từ đây" cho người/agent kế tiếp. Giữ nó là nguồn sự
> thật prose; cập nhật khi trạng thái dự án đổi. Trạng thái số liệu tự sinh ở
> [STATUS.md](STATUS.md).

## Dự án là gì
**LAB01_PRU213** — game 2D "Space Explorer" (Unity 6, URP 2D) cho Lab 1 môn PRU213.
Ngôn ngữ: python · Framework: unity · Domains: game

## Trạng thái hiện tại
- Bài nộp đã đủ theo đề: 3 scene (MainMenu → Gameplay → EndGame) có trong Build
  Settings, 7 prefab, 33 file C#, 54 texture PNG; README.md ở gốc repo tóm tắt game,
  điều khiển và bảng đối chiếu yêu cầu; báo cáo đầy đủ ở
  `Space Explorer - Documentation.docx` (mục 7 = mapping yêu cầu, mục 9 = hạn chế).
- Compile: `dotnet build SpaceExplorer.csproj` → 0 error, 40 warning CS0649 (field
  `[SerializeField]` gán qua Inspector — bình thường với Unity).
- Đã bỏ dependency local `com.coplaydev.unity-mcp` khỏi `Packages/manifest.json` +
  `packages-lock.json` để project mở được trên máy khác. Cần lại MCP thì thêm dòng
  `"com.coplaydev.unity-mcp": "file:D:/Edu/PackageGame/unity-mcp/MCPForUnity"`.
- Chưa chạy play-through trong Editor và chưa có test tự động.

## Cách chạy
- Mở project bằng Unity **6000.4.0f1** → mở `Assets/Scenes/MainMenu.unity` → Play.
- Sinh lại prefab + scene: menu `Space Explorer > Build Game` (ghi đè mọi sửa tay
  trong `Assets/Prefabs`, `Assets/Scenes`).
- Kiểm tra biên dịch: `dotnet build SpaceExplorer.csproj` và
  `dotnet build Assembly-CSharp-Editor.csproj`.
