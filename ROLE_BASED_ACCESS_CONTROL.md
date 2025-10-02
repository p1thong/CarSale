# Role-Based Access Control (RBAC) Implementation

## Actors và Permissions

### 🚗 Customer (Khách hàng)
**Capabilities:**
- ✅ Đặt lịch lái thử xe (Schedule Test Drive)
- ✅ Gửi phản hồi sau khi trải nghiệm (Submit Feedback)
- ✅ Xem dashboard cá nhân
- ✅ Quản lý đơn hàng và báo giá của mình

**UI Access:**
- Navigation menu hiển thị "My Services" với options:
  - Schedule Test Drive
  - My Dashboard
  - My Quotations
  - My Orders
- Test Drive Actions:
  - ❌ KHÔNG thấy nút "Xác nhận" hoặc "Hoàn thành"
  - ✅ Thấy nút "Phản hồi" khi test drive hoàn thành
  - ✅ Thấy nút "Đặt lại" để book lại

### 👨‍💼 Dealer Staff (Nhân viên đại lý)
**Capabilities:**
- ✅ Quản lý và xác nhận lịch lái thử (Manage & Confirm Test Drives)
- ✅ Đánh dấu hoàn thành lịch lái thử (Complete Test Drives)
- ✅ Xem tất cả feedbacks
- ✅ Xem báo cáo customer care
- ✅ Hỗ trợ customers đặt lịch

**UI Access:**
- Navigation menu hiển thị "Customer Service" với options:
  - Manage Test Drives
  - View All Feedbacks
  - Customer Care Report
- Test Drive Actions:
  - ✅ Thấy nút "Xác nhận" cho test drives scheduled
  - ✅ Thấy nút "Hoàn thành" cho test drives confirmed
  - ❌ KHÔNG thấy nút "Phản hồi" (customers tự gửi)
  - ✅ Thấy tất cả actions management

## Implementation Details

### 🔐 Authorization Attributes

```csharp
// Controller method authorization
[DealerStaffOnly]           // Only DealerManager, DealerStaff
[CustomerOnly]              // Only Customer
[CustomerAndDealer]         // Customer, DealerManager, DealerStaff
```

### 🎯 Applied Authorization

| Action | Customer | Dealer Staff | Description |
|--------|----------|--------------|-------------|
| `TestDrives` (View All) | ❌ | ✅ | Dealer staff manages all test drives |
| `ScheduleTestDrive` | ✅ | ✅ | Both can schedule (customer for self, staff for customers) |
| `ConfirmTestDrive` | ❌ | ✅ | Only dealer staff confirms bookings |
| `CompleteTestDrive` | ❌ | ✅ | Only dealer staff marks as completed |
| `CreateFeedback` | ✅ | ✅ | Customer creates, staff can help |
| `Feedbacks` (View All) | ❌ | ✅ | Dealer staff views all feedback |

### 🖥️ UI Role-Based Visibility

#### Test Drive Actions (TestDrives.cshtml)
```razor
@* Dealer Staff Only - Xác nhận và hoàn thành *@
@if (ViewBag.UserRole == "DealerManager" || ViewBag.UserRole == "DealerStaff")
{
    @if (testDrive.Status == "Scheduled")
    {
        <button onclick="confirmTestDrive(@testDrive.TestDriveId)">Xác nhận</button>
    }
    @if (testDrive.Status == "Confirmed")
    {
        <button onclick="completeTestDrive(@testDrive.TestDriveId)">Hoàn thành</button>
    }
}

@* Customer Only - Gửi phản hồi *@
@if (ViewBag.UserRole == "Customer" && testDrive.Status == "Completed")
{
    <a href="@Url.Action("CreateFeedback", ...)">Phản hồi</a>
}
```

#### Navigation Menu (_Layout.cshtml)
```razor
@if (ViewBag.UserRole == "DealerManager" || ViewBag.UserRole == "DealerStaff")
{
    <li>Customer Service</li>
    <ul>
        <li>Manage Test Drives</li>
        <li>View All Feedbacks</li>
        <li>Customer Care Report</li>
    </ul>
}

@if (ViewBag.UserRole == "Customer")
{
    <li>My Services</li>
    <ul>
        <li>Schedule Test Drive</li>
        <li>My Dashboard</li>
        <li>My Orders</li>
    </ul>
}
```

### 🔄 Complete User Workflow

#### Customer Journey:
1. **Login** → Customer role detected
2. **Navigation** → Shows "My Services" menu
3. **Schedule Test Drive** → Customer books a test drive
4. **Wait** → Test drive shows "Scheduled" status  
5. **Dealer confirms** → Status becomes "Confirmed"
6. **Dealer completes** → Status becomes "Completed"
7. **Customer feedback** → "Phản hồi" button appears
8. **Submit feedback** → Star rating and comments

#### Dealer Staff Journey:
1. **Login** → Dealer role detected
2. **Navigation** → Shows "Customer Service" menu
3. **Manage Test Drives** → View all customer bookings
4. **Confirm bookings** → Click "Xác nhận" for scheduled drives
5. **Complete drives** → Click "Hoàn thành" after customer experience
6. **View feedback** → Check customer feedback in system

### 🛡️ Security Features

- **Server-side validation**: All authorization checked in controller methods
- **UI security**: Buttons hidden based on roles, but backend still validates
- **Session management**: User roles stored securely in session
- **Claims-based auth**: Uses ASP.NET Core Claims for role management

### ⚡ Key Benefits

1. **Clear separation of duties**: Customers and staff have distinct responsibilities
2. **Security by design**: Authorization at both UI and API level
3. **User experience**: Each role sees only relevant options
4. **Scalable**: Easy to add new roles or modify permissions
5. **Maintainable**: Centralized authorization logic

## Testing Scenarios

### Test as Customer:
1. Login with customer account
2. Navigate to "My Services" → "Schedule Test Drive"
3. Book a test drive
4. Verify you DON'T see "Xác nhận" or "Hoàn thành" buttons
5. After dealer completes → Verify "Phản hồi" button appears

### Test as Dealer Staff:
1. Login with dealer staff account  
2. Navigate to "Customer Service" → "Manage Test Drives"
3. See all customer bookings
4. Verify you CAN see "Xác nhận" and "Hoàn thành" buttons
5. Complete a test drive and verify customer can then submit feedback

This implementation ensures proper role separation while maintaining a smooth user experience for both customers and dealer staff! 🚗⭐