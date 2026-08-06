# Nhắn tin realtime (SYS-MSG) — Digi parity

| | |
|---|---|
| Mã | `SPEC-MSG-RT-v2` |
| Cập nhật | 04/08/2026 |
| Tham chiếu | Digi `ChatHub` + `ChatPage` · ERP `/api/sys/msg` + `/hubs/msg` |
| Trạng thái | Digi-parity slice (1-1 · nhóm · edit · reply · recall · react · mute · typing · attach · members) |

## Mô hình (giống Digi)

- REST ghi DB → fan-out SignalR tới `user:{userId}`
- Hub mỏng: `JoinConversation` · `LeaveConversation` · `SendTypingStatus`
- Thu hồi mềm (`RecalledAt`) · sửa tin (`IsEdited`) · trả lời (`ParentMessageId`) · react (`chat_message_reaction`)
- Unread theo `ConversationMember.LastReadAt` · mute bỏ khỏi badge

## REST

| Method | Path | Perm |
|---|---|---|
| GET/POST | `/api/sys/msg/conversations` | read / send |
| GET/POST | `/api/sys/msg/conversations/{id}/messages` | read / send |
| PUT | `/api/sys/msg/conversations/{id}/messages/{messageId}` | send (edit) |
| POST | `…/messages/{messageId}/recall` | send |
| POST | `…/messages/{messageId}/reactions` | send (toggle emoji) |
| POST | `…/read` · `…/mute` | read |
| GET/POST/DELETE | `…/members` | read / send |
| GET | `/unread-count` · `/directory` | read |

## SignalR `/hubs/msg`

| Event | Hướng |
|---|---|
| `messageReceived` | Server → client (gửi + thu hồi) |
| `messageEdited` | Server → client |
| `reactionToggled` | Server → client |
| `conversationUpdated` | Server → client |
| `ReceiveTypingStatus` | Server → client (conv group) |
| `JoinConversation` / `LeaveConversation` / `SendTypingStatus` | Client → hub |

## FE

`/app/sys/messages` + popup `FloatingChatWindow`: bubble · trả lời / sửa / thu hồi / react · đính kèm · mute · thành viên nhóm.
