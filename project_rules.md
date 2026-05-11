# Tech Stack & Core Libraries
- Framework: .NET 9.0 (Windows Desktop)
- UI: Windows Forms (WinForms)
- Networking: System.Net.Sockets (TCP), Async/Await
- Serialization: System.Text.Json
- Interop: P/Invoke (user32.dll) via System.Runtime.InteropServices
- Hardware Info: System.Management (WMI)

# Architecture & Data Flow
- Architecture: Client-Relay-Server (Viewer -> Relay <- Host)
- Data Flow (Control): Client UI -> PacketBuilder -> AsyncTcpClient -> Relay -> AsyncTcpClient -> InputSimulator (Win32)
- Data Flow (Stream): ScreenCapturer -> JpegCompression -> PacketBuilder -> AsyncTcpClient -> Relay -> AsyncTcpClient -> ImageHelper -> Client UI
- Authentication: Hardware UUID -> Hash (Partner ID) + Random Password (One-time). Both checked at Relay.

# Directory Structure
- /NetworkCore: Shared library.
  - /Events: Custom EventArgs (DataReceived, ClientDisconnected).
  - /Protocol: Enums (DataType), PacketBuilder, PacketParser, Models (SessionPacket).
- /SharpView.Relay: Dockerized Console App. Bridges TCP streams (ConcurrentDictionary).
- /SharpView.Server: WinForms Host. Captures screen, simulates input.
  - /Helpers: ScreenCapturer, InputSimulator.
- /SharpView.Client: WinForms Viewer. Renders image, captures mouse/keyboard.
  - /Helpers: MouseScaler, ImageHelper.

# Coding Standards & Conventions
- Naming Conventions:
  - Classes/Methods: PascalCase.
  - Private fields: `_camelCase`.
  - Parameters/Local variables: `camelCase`.
- Design Principles:
  - SRP: Network, UI, and Parsing logic strictly separated.
  - Event-Driven: Network core communicates via Events (`DataReceived`).
- Error Handling & Logging:
  - UI Logging: Use `AppendLog()` safely via `Invoke`.
  - Connection Drops: Clean up Sockets immediately via `CleanupClient()` / `StopServer()`.

# Strict Rules (Do's and Don'ts)
- DO update UI exclusively via `SafeInvoke()` (Cross-thread safety).
- DO use `async/await` for all network I/O (`ReadAsync`, `WriteAsync`).
- DO dispose `IDisposable` objects (Bitmaps, Sockets) explicitly.
- DO NOT block the UI Thread (Stream loop must be a background Task).
- DO NOT hard-code IPs/Ports in production logic.
- DO NOT pass raw bytes directly to UI; always use `PacketParser` / `PacketBuilder`.
