using Erp.Domain.Entities.Ast;
using Erp.Domain.Entities.Bi;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Prt;
using Erp.Domain.Entities.Fin;
using Erp.Domain.Entities.Fsm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Log;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Mfg;
using Erp.Domain.Entities.Mod;
using Erp.Domain.Entities.Pjm;
using Erp.Domain.Entities.Pos;
using Erp.Domain.Entities.Pur;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Entities.Wf;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<JobLevel> JobLevels => Set<JobLevel>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<UserDepartment> UserDepartments => Set<UserDepartment>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserDataScope> UserDataScopes => Set<UserDataScope>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<LicenseModule> LicenseModules => Set<LicenseModule>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatMessageReaction> ChatMessageReactions => Set<ChatMessageReaction>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<LoginAudit> LoginAudits => Set<LoginAudit>();
    public DbSet<LookupCategory> LookupCategories => Set<LookupCategory>();
    public DbSet<LookupItem> LookupItems => Set<LookupItem>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<SysTrustedDevice> TrustedDevices => Set<SysTrustedDevice>();
    public DbSet<LegalEntity> LegalEntities => Set<LegalEntity>();
    public DbSet<SalesPoint> SalesPoints => Set<SalesPoint>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<WorkCalendar> WorkCalendars => Set<WorkCalendar>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<IntegrationCallLog> IntegrationCallLogs => Set<IntegrationCallLog>();
    public DbSet<PermissionChangeLog> PermissionChangeLogs => Set<PermissionChangeLog>();
    public DbSet<LocalePack> LocalePacks => Set<LocalePack>();
    public DbSet<FileObject> FileObjects => Set<FileObject>();
    public DbSet<FileFolder> FileFolders => Set<FileFolder>();
    public DbSet<ExternalIntegration> ExternalIntegrations => Set<ExternalIntegration>();

    public DbSet<JobTitle> JobTitles => Set<JobTitle>();
    public DbSet<EmployeeType> EmployeeTypes => Set<EmployeeType>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveEntitlementRule> LeaveEntitlementRules => Set<LeaveEntitlementRule>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<RecruitmentRequest> RecruitmentRequests => Set<RecruitmentRequest>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<OnboardingSetting> OnboardingSettings => Set<OnboardingSetting>();
    public DbSet<OnboardingCase> OnboardingCases => Set<OnboardingCase>();
    public DbSet<OnboardingDocument> OnboardingDocuments => Set<OnboardingDocument>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<HeadcountPlan> HeadcountPlans => Set<HeadcountPlan>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<ShiftPeriodLock> ShiftPeriodLocks => Set<ShiftPeriodLock>();
    public DbSet<StaffTransfer> StaffTransfers => Set<StaffTransfer>();
    public DbSet<AttendancePolicy> AttendancePolicies => Set<AttendancePolicy>();
    public DbSet<AttendanceDevice> AttendanceDevices => Set<AttendanceDevice>();
    public DbSet<AttendanceGeoFence> AttendanceGeoFences => Set<AttendanceGeoFence>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceAdjustRequest> AttendanceAdjustRequests => Set<AttendanceAdjustRequest>();
    public DbSet<AttendancePeriodLock> AttendancePeriodLocks => Set<AttendancePeriodLock>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<EmploymentStatusHistory> EmploymentStatusHistories => Set<EmploymentStatusHistory>();
    public DbSet<EmploymentStatusChange> EmploymentStatusChanges => Set<EmploymentStatusChange>();
    public DbSet<SalaryGrade> SalaryGrades => Set<SalaryGrade>();
    public DbSet<EmployeeSalary> EmployeeSalaries => Set<EmployeeSalary>();
    public DbSet<AllowanceType> AllowanceTypes => Set<AllowanceType>();
    public DbSet<AllowanceRule> AllowanceRules => Set<AllowanceRule>();
    public DbSet<PayrollPolicy> PayrollPolicies => Set<PayrollPolicy>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollLine> PayrollLines => Set<PayrollLine>();
    public DbSet<PayrollAdjustment> PayrollAdjustments => Set<PayrollAdjustment>();
    public DbSet<RewardDisciplineDecision> RewardDisciplineDecisions => Set<RewardDisciplineDecision>();
    public DbSet<OffboardingSetting> OffboardingSettings => Set<OffboardingSetting>();
    public DbSet<OffboardingCase> OffboardingCases => Set<OffboardingCase>();

    public DbSet<LmsTrainingClass> LmsTrainingClasses => Set<LmsTrainingClass>();
    public DbSet<LmsClassSession> LmsClassSessions => Set<LmsClassSession>();
    public DbSet<LmsClassEnrollment> LmsClassEnrollments => Set<LmsClassEnrollment>();
    public DbSet<LmsSessionAttendance> LmsSessionAttendances => Set<LmsSessionAttendance>();
    public DbSet<LmsMentorAssignment> LmsMentorAssignments => Set<LmsMentorAssignment>();
    public DbSet<LmsProgram> LmsPrograms => Set<LmsProgram>();
    public DbSet<LmsCourse> LmsCourses => Set<LmsCourse>();
    public DbSet<LmsChapter> LmsChapters => Set<LmsChapter>();
    public DbSet<LmsLesson> LmsLessons => Set<LmsLesson>();
    public DbSet<LmsOnlineEnrollment> LmsOnlineEnrollments => Set<LmsOnlineEnrollment>();
    public DbSet<LmsLessonProgress> LmsLessonProgresses => Set<LmsLessonProgress>();
    public DbSet<LmsQuestion> LmsQuestions => Set<LmsQuestion>();
    public DbSet<LmsExam> LmsExams => Set<LmsExam>();
    public DbSet<LmsExamQuestion> LmsExamQuestions => Set<LmsExamQuestion>();
    public DbSet<LmsExamAttempt> LmsExamAttempts => Set<LmsExamAttempt>();
    public DbSet<LmsCertificate> LmsCertificates => Set<LmsCertificate>();
    public DbSet<LmsInstructor> LmsInstructors => Set<LmsInstructor>();

    public DbSet<CrmCustomer> CrmCustomers => Set<CrmCustomer>();
    public DbSet<CrmContact> CrmContacts => Set<CrmContact>();
    public DbSet<CrmCustomerHandover> CrmCustomerHandovers => Set<CrmCustomerHandover>();
    public DbSet<CrmLeadSource> CrmLeadSources => Set<CrmLeadSource>();
    public DbSet<CrmLead> CrmLeads => Set<CrmLead>();
    public DbSet<CrmLeadTask> CrmLeadTasks => Set<CrmLeadTask>();
    public DbSet<CrmLeadActivity> CrmLeadActivities => Set<CrmLeadActivity>();
    public DbSet<CrmOpportunity> CrmOpportunities => Set<CrmOpportunity>();
    public DbSet<CrmOpportunityLine> CrmOpportunityLines => Set<CrmOpportunityLine>();
    public DbSet<CrmQuote> CrmQuotes => Set<CrmQuote>();
    public DbSet<CrmQuoteLine> CrmQuoteLines => Set<CrmQuoteLine>();
    public DbSet<CrmPriceList> CrmPriceLists => Set<CrmPriceList>();
    public DbSet<CrmPriceListItem> CrmPriceListItems => Set<CrmPriceListItem>();
    public DbSet<CrmSalesOrder> CrmSalesOrders => Set<CrmSalesOrder>();
    public DbSet<CrmSalesOrderLine> CrmSalesOrderLines => Set<CrmSalesOrderLine>();
    public DbSet<CrmOrderPayment> CrmOrderPayments => Set<CrmOrderPayment>();
    public DbSet<CrmCampaign> CrmCampaigns => Set<CrmCampaign>();
    public DbSet<CrmCampaignExpense> CrmCampaignExpenses => Set<CrmCampaignExpense>();
    public DbSet<CrmWebLead> CrmWebLeads => Set<CrmWebLead>();
    public DbSet<CrmPromotion> CrmPromotions => Set<CrmPromotion>();
    public DbSet<CrmPromotionCondition> CrmPromotionConditions => Set<CrmPromotionCondition>();
    public DbSet<CrmVoucher> CrmVouchers => Set<CrmVoucher>();
    public DbSet<CrmVoucherUsage> CrmVoucherUsages => Set<CrmVoucherUsage>();
    public DbSet<CrmChatHistory> CrmChatHistories => Set<CrmChatHistory>();

    public DbSet<PosStore> PosStores => Set<PosStore>();
    public DbSet<PosTerminal> PosTerminals => Set<PosTerminal>();
    public DbSet<PosPrinter> PosPrinters => Set<PosPrinter>();
    public DbSet<PosCashierAssignment> PosCashierAssignments => Set<PosCashierAssignment>();
    public DbSet<PosProductCategory> PosProductCategories => Set<PosProductCategory>();
    public DbSet<PosProduct> PosProducts => Set<PosProduct>();
    public DbSet<PosBomLine> PosBomLines => Set<PosBomLine>();
    public DbSet<PosTaxRate> PosTaxRates => Set<PosTaxRate>();
    public DbSet<PosPriceList> PosPriceLists => Set<PosPriceList>();
    public DbSet<PosPriceListItem> PosPriceListItems => Set<PosPriceListItem>();
    public DbSet<PosShift> PosShifts => Set<PosShift>();
    public DbSet<PosSale> PosSales => Set<PosSale>();
    public DbSet<PosSaleLine> PosSaleLines => Set<PosSaleLine>();
    public DbSet<PosSalePayment> PosSalePayments => Set<PosSalePayment>();
    public DbSet<PosReturn> PosReturns => Set<PosReturn>();
    public DbSet<PosReturnLine> PosReturnLines => Set<PosReturnLine>();
    public DbSet<PosPromotion> PosPromotions => Set<PosPromotion>();
    public DbSet<PosVoucher> PosVouchers => Set<PosVoucher>();

    public DbSet<PurVendor> PurVendors => Set<PurVendor>();
    public DbSet<PurVendorContact> PurVendorContacts => Set<PurVendorContact>();
    public DbSet<PurVendorProduct> PurVendorProducts => Set<PurVendorProduct>();
    public DbSet<PurVendorPrice> PurVendorPrices => Set<PurVendorPrice>();
    public DbSet<PurPurchaseRequest> PurPurchaseRequests => Set<PurPurchaseRequest>();
    public DbSet<PurPrLine> PurPrLines => Set<PurPrLine>();
    public DbSet<PurPurchaseOrder> PurPurchaseOrders => Set<PurPurchaseOrder>();
    public DbSet<PurPoLine> PurPoLines => Set<PurPoLine>();
    public DbSet<PurGoodsReceipt> PurGoodsReceipts => Set<PurGoodsReceipt>();
    public DbSet<PurGrnLine> PurGrnLines => Set<PurGrnLine>();
    public DbSet<PurVendorInvoice> PurVendorInvoices => Set<PurVendorInvoice>();
    public DbSet<PurInvoiceLine> PurInvoiceLines => Set<PurInvoiceLine>();

    public DbSet<InvItemGroup> InvItemGroups => Set<InvItemGroup>();
    public DbSet<InvUnitOfMeasure> InvUnitsOfMeasure => Set<InvUnitOfMeasure>();
    public DbSet<InvUnitConversion> InvUnitConversions => Set<InvUnitConversion>();
    public DbSet<InvSku> InvSkus => Set<InvSku>();
    public DbSet<InvWarehouseType> InvWarehouseTypes => Set<InvWarehouseType>();
    public DbSet<InvWarehouse> InvWarehouses => Set<InvWarehouse>();
    public DbSet<InvWarehouseKeeper> InvWarehouseKeepers => Set<InvWarehouseKeeper>();
    public DbSet<InvStockBalance> InvStockBalances => Set<InvStockBalance>();
    public DbSet<InvStockDoc> InvStockDocs => Set<InvStockDoc>();
    public DbSet<InvStockDocLine> InvStockDocLines => Set<InvStockDocLine>();
    public DbSet<InvTransfer> InvTransfers => Set<InvTransfer>();
    public DbSet<InvTransferLine> InvTransferLines => Set<InvTransferLine>();
    public DbSet<InvStocktake> InvStocktakes => Set<InvStocktake>();
    public DbSet<InvStocktakeLine> InvStocktakeLines => Set<InvStocktakeLine>();
    public DbSet<InvStockReservation> InvStockReservations => Set<InvStockReservation>();
    public DbSet<InvStockReservationLine> InvStockReservationLines => Set<InvStockReservationLine>();

    public DbSet<LogCarrier> LogCarriers => Set<LogCarrier>();
    public DbSet<LogDeliveryOrder> LogDeliveryOrders => Set<LogDeliveryOrder>();
    public DbSet<LogDeliveryLine> LogDeliveryLines => Set<LogDeliveryLine>();
    public DbSet<LogShipmentEvent> LogShipmentEvents => Set<LogShipmentEvent>();
    public DbSet<LogCodHandover> LogCodHandovers => Set<LogCodHandover>();
    public DbSet<LogCodHandoverLine> LogCodHandoverLines => Set<LogCodHandoverLine>();
    public DbSet<LogReturnNote> LogReturnNotes => Set<LogReturnNote>();
    public DbSet<LogReturnLine> LogReturnLines => Set<LogReturnLine>();

    public DbSet<MfgItem> MfgItems => Set<MfgItem>();
    public DbSet<MfgWorkshop> MfgWorkshops => Set<MfgWorkshop>();
    public DbSet<MfgBom> MfgBoms => Set<MfgBom>();
    public DbSet<MfgBomLine> MfgBomLines => Set<MfgBomLine>();
    public DbSet<MfgPlan> MfgPlans => Set<MfgPlan>();
    public DbSet<MfgPlanLine> MfgPlanLines => Set<MfgPlanLine>();
    public DbSet<MfgWorkOrder> MfgWorkOrders => Set<MfgWorkOrder>();
    public DbSet<MfgMaterialIssue> MfgMaterialIssues => Set<MfgMaterialIssue>();
    public DbSet<MfgFgReceipt> MfgFgReceipts => Set<MfgFgReceipt>();
    public DbSet<MfgScrap> MfgScraps => Set<MfgScrap>();
    public DbSet<MfgCostSheet> MfgCostSheets => Set<MfgCostSheet>();
    public DbSet<MfgCostSheetLine> MfgCostSheetLines => Set<MfgCostSheetLine>();

    public DbSet<FsmServiceType> FsmServiceTypes => Set<FsmServiceType>();
    public DbSet<FsmFaultCode> FsmFaultCodes => Set<FsmFaultCode>();
    public DbSet<FsmPart> FsmParts => Set<FsmPart>();
    public DbSet<FsmPartStock> FsmPartStocks => Set<FsmPartStock>();
    public DbSet<FsmPartIssueDoc> FsmPartIssueDocs => Set<FsmPartIssueDoc>();
    public DbSet<FsmPartIssueLine> FsmPartIssueLines => Set<FsmPartIssueLine>();
    public DbSet<FsmPartReconcileDoc> FsmPartReconcileDocs => Set<FsmPartReconcileDoc>();
    public DbSet<FsmPartReconcileLine> FsmPartReconcileLines => Set<FsmPartReconcileLine>();
    public DbSet<FsmTicketPartLine> FsmTicketPartLines => Set<FsmTicketPartLine>();
    public DbSet<FsmSlaPolicy> FsmSlaPolicies => Set<FsmSlaPolicy>();
    public DbSet<FsmAsset> FsmAssets => Set<FsmAsset>();
    public DbSet<FsmAssetHistory> FsmAssetHistories => Set<FsmAssetHistory>();
    public DbSet<FsmTicket> FsmTickets => Set<FsmTicket>();

    public DbSet<PjmProjectType> PjmProjectTypes => Set<PjmProjectType>();
    public DbSet<PjmProjectStatus> PjmProjectStatuses => Set<PjmProjectStatus>();
    public DbSet<PjmWbsTemplate> PjmWbsTemplates => Set<PjmWbsTemplate>();
    public DbSet<PjmWbsTemplateItem> PjmWbsTemplateItems => Set<PjmWbsTemplateItem>();
    public DbSet<PjmProject> PjmProjects => Set<PjmProject>();
    public DbSet<PjmProjectMember> PjmProjectMembers => Set<PjmProjectMember>();
    public DbSet<PjmWbsItem> PjmWbsItems => Set<PjmWbsItem>();
    public DbSet<PjmExpense> PjmExpenses => Set<PjmExpense>();
    public DbSet<PjmMaterialIssue> PjmMaterialIssues => Set<PjmMaterialIssue>();
    public DbSet<PjmMaterialIssueLine> PjmMaterialIssueLines => Set<PjmMaterialIssueLine>();
    public DbSet<PjmAcceptance> PjmAcceptances => Set<PjmAcceptance>();

    public DbSet<FinAccountGroup> FinAccountGroups => Set<FinAccountGroup>();
    public DbSet<FinAccount> FinAccounts => Set<FinAccount>();
    public DbSet<FinFiscalYear> FinFiscalYears => Set<FinFiscalYear>();
    public DbSet<FinPeriod> FinPeriods => Set<FinPeriod>();
    public DbSet<FinCostCenter> FinCostCenters => Set<FinCostCenter>();
    public DbSet<FinPaymentMethod> FinPaymentMethods => Set<FinPaymentMethod>();
    public DbSet<FinTax> FinTaxes => Set<FinTax>();
    public DbSet<FinVatDocument> FinVatDocuments => Set<FinVatDocument>();
    public DbSet<FinRevenueDocument> FinRevenueDocuments => Set<FinRevenueDocument>();
    public DbSet<FinJournal> FinJournals => Set<FinJournal>();
    public DbSet<FinJournalLine> FinJournalLines => Set<FinJournalLine>();
    public DbSet<FinCashFund> FinCashFunds => Set<FinCashFund>();
    public DbSet<FinCashVoucher> FinCashVouchers => Set<FinCashVoucher>();
    public DbSet<FinBankAccount> FinBankAccounts => Set<FinBankAccount>();
    public DbSet<FinBankVoucher> FinBankVouchers => Set<FinBankVoucher>();
    public DbSet<FinBankTransferRequest> FinBankTransferRequests => Set<FinBankTransferRequest>();
    public DbSet<FinBankStatementLine> FinBankStatementLines => Set<FinBankStatementLine>();
    public DbSet<FinApInvoice> FinApInvoices => Set<FinApInvoice>();
    public DbSet<FinApPaymentRequest> FinApPaymentRequests => Set<FinApPaymentRequest>();
    public DbSet<FinApPaymentRequestLine> FinApPaymentRequestLines => Set<FinApPaymentRequestLine>();
    public DbSet<FinApPayment> FinApPayments => Set<FinApPayment>();
    public DbSet<FinApPaymentAllocation> FinApPaymentAllocations => Set<FinApPaymentAllocation>();
    public DbSet<FinArInvoice> FinArInvoices => Set<FinArInvoice>();
    public DbSet<FinArCreditLimit> FinArCreditLimits => Set<FinArCreditLimit>();
    public DbSet<FinArReceipt> FinArReceipts => Set<FinArReceipt>();
    public DbSet<FinArReceiptAllocation> FinArReceiptAllocations => Set<FinArReceiptAllocation>();

    public DbSet<AstAssetGroup> AstAssetGroups => Set<AstAssetGroup>();
    public DbSet<AstLocation> AstLocations => Set<AstLocation>();
    public DbSet<AstDepreciationMethod> AstDepreciationMethods => Set<AstDepreciationMethod>();
    public DbSet<AstAsset> AstAssets => Set<AstAsset>();
    public DbSet<AstMovementDoc> AstMovementDocs => Set<AstMovementDoc>();
    public DbSet<AstStocktake> AstStocktakes => Set<AstStocktake>();
    public DbSet<AstStocktakeLine> AstStocktakeLines => Set<AstStocktakeLine>();
    public DbSet<AstDepreciationRun> AstDepreciationRuns => Set<AstDepreciationRun>();
    public DbSet<AstDepreciationLine> AstDepreciationLines => Set<AstDepreciationLine>();

    public DbSet<BiDataset> BiDatasets => Set<BiDataset>();
    public DbSet<BiDatasetRefresh> BiDatasetRefreshes => Set<BiDatasetRefresh>();
    public DbSet<BiReport> BiReports => Set<BiReport>();
    public DbSet<BiReportPermission> BiReportPermissions => Set<BiReportPermission>();
    public DbSet<BiDashboard> BiDashboards => Set<BiDashboard>();
    public DbSet<BiWidget> BiWidgets => Set<BiWidget>();
    public DbSet<BiReportRun> BiReportRuns => Set<BiReportRun>();
    public DbSet<BiKpiTarget> BiKpiTargets => Set<BiKpiTarget>();
    public DbSet<BiAlertThreshold> BiAlertThresholds => Set<BiAlertThreshold>();

    public DbSet<PrtAccount> PrtAccounts => Set<PrtAccount>();
    public DbSet<PrtOrder> PrtOrders => Set<PrtOrder>();
    public DbSet<PrtOrderLine> PrtOrderLines => Set<PrtOrderLine>();
    public DbSet<PrtInvoice> PrtInvoices => Set<PrtInvoice>();
    public DbSet<PrtPayment> PrtPayments => Set<PrtPayment>();
    public DbSet<PrtTicket> PrtTickets => Set<PrtTicket>();
    public DbSet<PrtPortalPackage> PrtPortalPackages => Set<PrtPortalPackage>();

    public DbSet<ModMaster> ModMasters => Set<ModMaster>();
    public DbSet<ModDocument> ModDocuments => Set<ModDocument>();

    public DbSet<WorkType> WorkTypes => Set<WorkType>();
    public DbSet<WorkProject> WorkProjects => Set<WorkProject>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    public DbSet<WfDefinition> WfDefinitions => Set<WfDefinition>();
    public DbSet<WfDefinitionVersion> WfDefinitionVersions => Set<WfDefinitionVersion>();
    public DbSet<WfNode> WfNodes => Set<WfNode>();
    public DbSet<WfInstance> WfInstances => Set<WfInstance>();
    public DbSet<WfTask> WfTasks => Set<WfTask>();
    public DbSet<WfTaskAction> WfTaskActions => Set<WfTaskAction>();
    public DbSet<WfDelegation> WfDelegations => Set<WfDelegation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQL Server đã dùng schema hệ thống `sys` — module SYS map sang `erp_sys`
        modelBuilder.HasDefaultSchema("erp_sys");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<WfDefinition>(b =>
        {
            b.ToTable("wf_definition", "wf");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.Property(x => x.Code).HasMaxLength(50);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.ModuleCode).HasMaxLength(10);
            b.Property(x => x.DocType).HasMaxLength(80);
        });
        modelBuilder.Entity<WfDefinitionVersion>(b =>
        {
            b.ToTable("wf_definition_version", "wf");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.DefinitionId, x.VersionNo }).IsUnique();
        });
        modelBuilder.Entity<WfNode>(b =>
        {
            b.ToTable("wf_node", "wf");
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).HasMaxLength(50);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.NodeType).HasMaxLength(30);
        });
        modelBuilder.Entity<WfInstance>(b =>
        {
            b.ToTable("wf_instance", "wf");
            b.HasKey(x => x.Id);
            b.Property(x => x.SourceModule).HasMaxLength(10);
            b.Property(x => x.SourceDocType).HasMaxLength(80);
            b.Property(x => x.Status).HasMaxLength(30);
        });
        modelBuilder.Entity<WfTask>(b =>
        {
            b.ToTable("wf_task", "wf");
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasMaxLength(30);
        });
        modelBuilder.Entity<WfTaskAction>(b =>
        {
            b.ToTable("wf_task_action", "wf");
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).HasMaxLength(30);
        });
        modelBuilder.Entity<WfDelegation>(b =>
        {
            b.ToTable("wf_delegation", "wf");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.TenantId, x.FromUserId, x.ToUserId, x.StartDate });
            b.HasIndex(x => new { x.TenantId, x.ToUserId, x.IsActive });
            b.Property(x => x.ModuleCode).HasMaxLength(10);
            b.Property(x => x.Note).HasMaxLength(500);
        });

        base.OnModelCreating(modelBuilder);
    }
}
