# technical-design — kiến trúc & quyết định (ADR)

> Ghi các quyết định kiến trúc kèm LÝ DO, để sau này không phải đoán. Mỗi quyết
> định một mục; lịch sử dạng dòng ở [decisions-log.md](decisions-log.md).

## AD-1: Stack phát hiện khi onboard
- **Bối cảnh:** `govern-init` profile project từ package.json/README/marker file lúc onboard governance.
- **Quyết định:** Ngôn ngữ: python. Framework/engine: unity. Domain: game.
- **Lý do / đánh đổi:** Verify commands dùng cho Stop-hook: lint=null, test=null, build=null. Sửa trực tiếp trong `config/governance.json` nếu phát hiện sai.
