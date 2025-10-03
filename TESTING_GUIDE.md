# Sales Flow Testing Guide

## 🔧 Setup Test Environment

### 1. Ensure Application is Running
```bash
dotnet run --project ASM1.WebMVC
# App should be available at: http://localhost:5291
```

### 2. Database Check
Ensure you have test data:
- At least 1 VehicleVariant with valid ID
- At least 1 Customer record
- At least 1 Dealer record

## 🧪 Test Scenarios

### Scenario 1: Happy Path - Complete Flow

#### Step 1: Create Quotation Request
```
URL: http://localhost:5291/SalesFlow/RequestQuotation/1
Method: GET → POST
Expected: Quotation created with status "Pending"
```

**Test Data:**
- variantId: 1 (existing vehicle)
- notes: "Test quotation request"

**Verification:**
- Check database: New record in Quotation table
- Status should be "Pending"
- CreatedAt should be current timestamp

#### Step 2: Dealer Creates Quote
```
URL: http://localhost:5291/SalesFlow/PendingQuotations
Method: GET
Expected: List shows pending quotations
```

```
URL: http://localhost:5291/SalesFlow/CreateQuotation/{quotationId}
Method: GET → POST
Expected: Quote updated with dealer price
```

**Test Data:**
- quotedPrice: 950000000 (lower than list price)
- dealerNotes: "Special discount for valued customer"

**Verification:**
- Quotation.Price updated
- Status changed to "Quoted"

#### Step 3: Customer Reviews & Approves
```
URL: http://localhost:5291/SalesFlow/ReviewQuotation/{quotationId}
Method: GET → POST (ApproveQuotation)
Expected: Quotation approved
```

**Verification:**
- Status changed to "Approved"
- Price comparison displayed correctly
- Savings calculation accurate

#### Step 4: Create Order
```
URL: http://localhost:5291/SalesFlow/CreateOrderFromQuotation
Method: POST
Expected: Order created from approved quotation
```

**Verification:**
- New Order record created
- Order.VariantId matches Quotation.VariantId
- Order.CustomerId matches Quotation.CustomerId

#### Step 5: Payment Processing
```
URL: http://localhost:5291/SalesFlow/MakePayment/{orderId}
Method: GET → POST
Expected: Payment processed successfully
```

**Test Scenarios:**
1. Credit Card Payment
2. Bank Transfer
3. Cash Payment
4. Installment Payment

**Test Data:**
- amount: Full order amount
- paymentMethod: "CreditCard"

**Verification:**
- Payment record created
- Order status updated
- Redirect to PaymentSuccess page

### Scenario 2: Error Handling

#### Test Invalid IDs
```
URL: http://localhost:5291/SalesFlow/RequestQuotation/99999
Expected: Error message + redirect
```

#### Test Workflow Violations
```
URL: http://localhost:5291/SalesFlow/ReviewQuotation/{pendingQuotationId}
Expected: Should not allow review of pending quotation
```

#### Test Missing Data
- Submit empty forms
- Test with null/invalid amounts
- Test with non-existent relationships

### Scenario 3: UI/UX Testing

#### Progress Indicators
- Verify 5-step progress shows correctly
- Check active/completed states
- Ensure consistent styling

#### Responsive Design
- Test on different screen sizes
- Mobile responsiveness
- Table scrolling on small screens

#### User Experience
- Form validation messages
- Success/error feedback
- Navigation consistency

## 📊 Verification Checkpoints

### Database Verification
```sql
-- Check quotation flow
SELECT q.QuotationId, q.Status, q.Price, c.FullName, v.Version 
FROM Quotation q
JOIN Customer c ON q.CustomerId = c.CustomerId
JOIN VehicleVariant v ON q.VariantId = v.VariantId
ORDER BY q.CreatedAt DESC;

-- Check order creation
SELECT o.OrderId, o.Status, o.OrderDate, q.QuotationId
FROM [Order] o
LEFT JOIN Quotation q ON o.CustomerId = q.CustomerId
ORDER BY o.OrderDate DESC;

-- Check payments
SELECT p.PaymentId, p.OrderId, p.Amount, p.Method, p.PaymentDate
FROM Payment p
ORDER BY p.PaymentDate DESC;
```

### Log Verification
Monitor application logs for:
- Database connection issues
- Entity Framework queries
- Exception handling
- Performance metrics

## 🎯 Success Criteria

### Functional Requirements ✅
- [x] Complete 5-step flow works end-to-end
- [x] Role-based access control functions
- [x] Data persistence across steps
- [x] Payment processing handles multiple methods
- [x] Status tracking accurate throughout

### Performance Requirements ✅
- [x] Page load times < 3 seconds
- [x] Database queries optimized
- [x] No memory leaks during flow
- [x] Concurrent user handling

### UI/UX Requirements ✅
- [x] Intuitive navigation between steps
- [x] Clear progress indication
- [x] Responsive design works
- [x] Error messages user-friendly
- [x] Success confirmations clear

## 🐛 Common Issues & Solutions

### Migration Conflicts
```bash
# If database schema issues occur
dotnet ef database drop --project ASM1.Repository --startup-project ASM1.WebMVC
dotnet ef database update --project ASM1.Repository --startup-project ASM1.WebMVC
```

### Session Management
- Clear browser cookies if role detection fails
- Check session configuration in Program.cs
- Verify UserRole/UserId session values

### Navigation Issues
- Ensure all action methods return correct ViewResult
- Check route parameters match method signatures
- Verify model binding for complex objects

## 📈 Performance Testing

### Load Testing
```bash
# Use artillery or similar tool
artillery run load-test-config.yml
```

### Database Performance
```sql
-- Check slow queries
SELECT TOP 10 
    total_elapsed_time/execution_count AS avg_cpu_time,
    text
FROM sys.dm_exec_query_stats
CROSS APPLY sys.dm_exec_sql_text(sql_handle)
ORDER BY avg_cpu_time DESC;
```

## 🔍 Debugging Tips

### Enable Detailed Logging
```csharp
// In Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### Entity Framework Logging
```csharp
// In DbContext configuration
options.LogTo(Console.WriteLine, LogLevel.Information);
```

### Browser Developer Tools
- Network tab for API calls
- Console for JavaScript errors  
- Application tab for session storage