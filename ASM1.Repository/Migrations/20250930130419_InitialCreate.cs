using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASM1.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dealer",
                columns: table => new
                {
                    dealerId = table.Column<int>(type: "int", nullable: false),
                    fullName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    transactionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Dealer__5A9E9D961C30970D", x => x.dealerId);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturer",
                columns: table => new
                {
                    manufacturerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    country = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Manufact__02B55389ED519028", x => x.manufacturerId);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    customerId = table.Column<int>(type: "int", nullable: false),
                    dealerId = table.Column<int>(type: "int", nullable: false),
                    fullName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    birthday = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__B611CB7D0049E38E", x => x.customerId);
                    table.ForeignKey(
                        name: "FK__Customer__dealer__6FE99F9F",
                        column: x => x.dealerId,
                        principalTable: "Dealer",
                        principalColumn: "dealerId");
                });

            migrationBuilder.CreateTable(
                name: "DealerContract",
                columns: table => new
                {
                    dealerContractId = table.Column<int>(type: "int", nullable: false),
                    dealerId = table.Column<int>(type: "int", nullable: false),
                    manufacturerId = table.Column<int>(type: "int", nullable: false),
                    targetSales = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    creditLimit = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    signedDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DealerCo__5D9704E720573FF3", x => x.dealerContractId);
                    table.ForeignKey(
                        name: "FK__DealerCon__deale__6C190EBB",
                        column: x => x.dealerId,
                        principalTable: "Dealer",
                        principalColumn: "dealerId");
                    table.ForeignKey(
                        name: "FK__DealerCon__manuf__6D0D32F4",
                        column: x => x.manufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "manufacturerId");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false),
                    fullName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    password = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    dealerId = table.Column<int>(type: "int", nullable: true),
                    manufacturerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__CB9A1CFF85902E89", x => x.userId);
                    table.ForeignKey(
                        name: "FK__User__dealerId__68487DD7",
                        column: x => x.dealerId,
                        principalTable: "Dealer",
                        principalColumn: "dealerId");
                    table.ForeignKey(
                        name: "FK__User__manufactur__693CA210",
                        column: x => x.manufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "manufacturerId");
                });

            migrationBuilder.CreateTable(
                name: "VehicleModel",
                columns: table => new
                {
                    vehicleModelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    manufacturerId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VehicleM__DF4B1849AF5DCC0A", x => x.vehicleModelId);
                    table.ForeignKey(
                        name: "FK__VehicleMo__manuf__72C60C4A",
                        column: x => x.manufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "manufacturerId");
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    feedbackId = table.Column<int>(type: "int", nullable: false),
                    customerId = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    FeedbackDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feedback__2613FD24ACBA58F5", x => x.feedbackId);
                    table.ForeignKey(
                        name: "FK__Feedback__custom__02FC7413",
                        column: x => x.customerId,
                        principalTable: "Customer",
                        principalColumn: "customerId");
                });

            migrationBuilder.CreateTable(
                name: "VehicleVariant",
                columns: table => new
                {
                    variantId = table.Column<int>(type: "int", nullable: false),
                    vehicleModelId = table.Column<int>(type: "int", nullable: false),
                    version = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    color = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    productYear = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<decimal>(type: "decimal(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VehicleV__69E44B2D7D537419", x => x.variantId);
                    table.ForeignKey(
                        name: "FK__VehicleVa__vehic__75A278F5",
                        column: x => x.vehicleModelId,
                        principalTable: "VehicleModel",
                        principalColumn: "vehicleModelId");
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    orderId = table.Column<int>(type: "int", nullable: false),
                    dealerId = table.Column<int>(type: "int", nullable: false),
                    customerId = table.Column<int>(type: "int", nullable: false),
                    variantId = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    orderDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Order__0809335DA5E72A85", x => x.orderId);
                    table.ForeignKey(
                        name: "FK__Order__customerI__797309D9",
                        column: x => x.customerId,
                        principalTable: "Customer",
                        principalColumn: "customerId");
                    table.ForeignKey(
                        name: "FK__Order__dealerId__787EE5A0",
                        column: x => x.dealerId,
                        principalTable: "Dealer",
                        principalColumn: "dealerId");
                    table.ForeignKey(
                        name: "FK__Order__variantId__7A672E12",
                        column: x => x.variantId,
                        principalTable: "VehicleVariant",
                        principalColumn: "variantId");
                });

            migrationBuilder.CreateTable(
                name: "Quotation",
                columns: table => new
                {
                    quotationId = table.Column<int>(type: "int", nullable: false),
                    customerId = table.Column<int>(type: "int", nullable: false),
                    variantId = table.Column<int>(type: "int", nullable: false),
                    dealerId = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Quotatio__7536E352113ABA0A", x => x.quotationId);
                    table.ForeignKey(
                        name: "FK__Quotation__custo__0C85DE4D",
                        column: x => x.customerId,
                        principalTable: "Customer",
                        principalColumn: "customerId");
                    table.ForeignKey(
                        name: "FK__Quotation__deale__0E6E26BF",
                        column: x => x.dealerId,
                        principalTable: "Dealer",
                        principalColumn: "dealerId");
                    table.ForeignKey(
                        name: "FK__Quotation__varia__0D7A0286",
                        column: x => x.variantId,
                        principalTable: "VehicleVariant",
                        principalColumn: "variantId");
                });

            migrationBuilder.CreateTable(
                name: "TestDrive",
                columns: table => new
                {
                    testDriveId = table.Column<int>(type: "int", nullable: false),
                    customerId = table.Column<int>(type: "int", nullable: false),
                    variantId = table.Column<int>(type: "int", nullable: false),
                    scheduledDate = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TestDriv__1BEFF08411737214", x => x.testDriveId);
                    table.ForeignKey(
                        name: "FK__TestDrive__custo__08B54D69",
                        column: x => x.customerId,
                        principalTable: "Customer",
                        principalColumn: "customerId");
                    table.ForeignKey(
                        name: "FK__TestDrive__varia__09A971A2",
                        column: x => x.variantId,
                        principalTable: "VehicleVariant",
                        principalColumn: "variantId");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    paymentId = table.Column<int>(type: "int", nullable: false),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    method = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    paymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payment__A0D9EFC6E641B2C2", x => x.paymentId);
                    table.ForeignKey(
                        name: "FK__Payment__orderId__7D439ABD",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                });

            migrationBuilder.CreateTable(
                name: "Promotion",
                columns: table => new
                {
                    promotionId = table.Column<int>(type: "int", nullable: false),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    discountAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    promotionCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    validUntil = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Promotio__99EB696E2E1B6388", x => x.promotionId);
                    table.ForeignKey(
                        name: "FK__Promotion__order__00200768",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                });

            migrationBuilder.CreateTable(
                name: "SalesContract",
                columns: table => new
                {
                    saleContractId = table.Column<int>(type: "int", nullable: false),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    signedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ContractDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SalesCon__BBA78B0BB1B2B884", x => x.saleContractId);
                    table.ForeignKey(
                        name: "FK__SalesCont__order__05D8E0BE",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customer_dealerId",
                table: "Customer",
                column: "dealerId");

            migrationBuilder.CreateIndex(
                name: "IX_DealerContract_dealerId",
                table: "DealerContract",
                column: "dealerId");

            migrationBuilder.CreateIndex(
                name: "IX_DealerContract_manufacturerId",
                table: "DealerContract",
                column: "manufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_customerId",
                table: "Feedback",
                column: "customerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_customerId",
                table: "Order",
                column: "customerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_dealerId",
                table: "Order",
                column: "dealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_variantId",
                table: "Order",
                column: "variantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_orderId",
                table: "Payment",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotion_orderId",
                table: "Promotion",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotation_customerId",
                table: "Quotation",
                column: "customerId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotation_dealerId",
                table: "Quotation",
                column: "dealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotation_variantId",
                table: "Quotation",
                column: "variantId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesContract_orderId",
                table: "SalesContract",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDrive_customerId",
                table: "TestDrive",
                column: "customerId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDrive_variantId",
                table: "TestDrive",
                column: "variantId");

            migrationBuilder.CreateIndex(
                name: "IX_User_dealerId",
                table: "User",
                column: "dealerId");

            migrationBuilder.CreateIndex(
                name: "IX_User_manufacturerId",
                table: "User",
                column: "manufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModel_manufacturerId",
                table: "VehicleModel",
                column: "manufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVariant_vehicleModelId",
                table: "VehicleVariant",
                column: "vehicleModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DealerContract");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Promotion");

            migrationBuilder.DropTable(
                name: "Quotation");

            migrationBuilder.DropTable(
                name: "SalesContract");

            migrationBuilder.DropTable(
                name: "TestDrive");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "VehicleVariant");

            migrationBuilder.DropTable(
                name: "Dealer");

            migrationBuilder.DropTable(
                name: "VehicleModel");

            migrationBuilder.DropTable(
                name: "Manufacturer");
        }
    }
}
