using Erp.Application.Interfaces.Realtime;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Application.Interfaces.Services.Ast;
using Erp.Application.Interfaces.Services.Bi;
using Erp.Application.Interfaces.Services.Crm;
using Erp.Application.Interfaces.Services.Prt;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Application.Interfaces.Services.Fsm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Application.Interfaces.Services.Inv;
using Erp.Application.Interfaces.Services.Log;
using Erp.Application.Interfaces.Services.Lms;
using Erp.Application.Interfaces.Services.Mfg;
using Erp.Application.Interfaces.Services.Mod;
using Erp.Application.Interfaces.Services.Pjm;
using Erp.Application.Interfaces.Services.Pos;
using Erp.Application.Interfaces.Services.Pur;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Application.Interfaces.Services.Wf;
using Erp.Infrastructure.Background;
using Erp.Infrastructure.Implementations.Services.Auth;
using Erp.Infrastructure.Implementations.Services.Ast;
using Erp.Infrastructure.Implementations.Services.Bi;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Implementations.Services.Prt;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Fsm;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Implementations.Services.Lms;
using Erp.Infrastructure.Implementations.Services.Mfg;
using Erp.Infrastructure.Implementations.Services.Mod;
using Erp.Infrastructure.Implementations.Services.Pjm;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Implementations.Services.Wf;
using Erp.Infrastructure.Persistence;
using Erp.Infrastructure.Persistence.Interceptors;
using Erp.Infrastructure.Security;
using Erp.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Erp.Infrastructure;

/// <summary>
/// Đăng ký DI theo module — cắt SKU = bỏ gọi AddXxxModule() (+ xóa folder, xem MODULES.md).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Default")
                 ?? config["CONNECTION_STRING"]
                 ?? throw new InvalidOperationException(
                     "Thiếu ConnectionStrings:Default hoặc CONNECTION_STRING (.env).");

        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<AppDbContext>((sp, opt) =>
        {
            opt.UseSqlServer(cs, sql => sql.MigrationsHistoryTable("__ef_migrations_history", "erp_sys"));
            opt.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        // Platform — luôn giữ khi bán
        services.AddSysModule();
        services.AddModKit();

        // Sellable — comment / xóa dòng khi cut source theo MODULES.json
        services.AddHrmModule();
        services.AddLmsModule();
        services.AddCrmModule();
        services.AddPosModule();
        services.AddPurModule();
        services.AddInvModule();
        services.AddLogModule();
        services.AddMfgModule();
        services.AddFsmModule();
        services.AddPjmModule();
        services.AddFinModule();
        services.AddAstModule();
        services.AddBiModule();
        services.AddPrtModule();
        services.AddWfModule();

        services.AddHttpClient("cloudinary");
        services.AddSingleton<IFileStorageService>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var cloud = cfg["CLOUDINARY_CLOUD_NAME"];
            if (!string.IsNullOrWhiteSpace(cloud)
                && !string.IsNullOrWhiteSpace(cfg["CLOUDINARY_API_KEY"])
                && !string.IsNullOrWhiteSpace(cfg["CLOUDINARY_API_SECRET"]))
            {
                return new CloudinaryFileStorageService(
                    sp.GetRequiredService<IHttpClientFactory>(),
                    cfg,
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CloudinaryFileStorageService>>());
            }
            return new LocalFileStorageService(cfg, sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>());
        });
        return services;
    }

    /// <summary>SYS + Auth + Msg + Outbox — không cắt.</summary>
    public static IServiceCollection AddSysModule(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IDataScopeService, DataScopeService>();
        services.AddScoped<ISysMasterService, SysMasterService>();
        services.AddScoped<ISysPlatformService, SysPlatformService>();
        services.AddScoped<IMsgService, MsgService>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IInboxStore, InboxStore>();
        services.AddHostedService<OutboxDispatcherHostedService>();
        return services;
    }

    /// <summary>Day-1 kit (masters/docs theo moduleCode) — nền stub CRM…PRT.</summary>
    public static IServiceCollection AddModKit(this IServiceCollection services)
    {
        services.AddScoped<IModModuleService, ModModuleService>();
        return services;
    }

    /// <summary>HRM — cắt khi khách không mua.</summary>
    public static IServiceCollection AddHrmModule(this IServiceCollection services)
    {
        services.AddScoped<IHrmEmployeeService, HrmEmployeeService>();
        services.AddScoped<IHrmLeaveService, HrmLeaveService>();
        services.AddScoped<IHrmRecruitService, HrmRecruitService>();
        services.AddScoped<IHrmRecruitPipelineService, HrmRecruitPipelineService>();
        services.AddScoped<IHrmOnboardingService, HrmOnboardingService>();
        services.AddScoped<IHrmHeadcountService, HrmHeadcountService>();
        services.AddScoped<IHrmShiftService, HrmShiftService>();
        services.AddScoped<IHrmTransferService, HrmTransferService>();
        services.AddScoped<IHrmAttendanceService, HrmAttendanceService>();
        services.AddScoped<IHrmContractService, HrmContractService>();
        services.AddScoped<IHrmPayrollService, HrmPayrollService>();
        services.AddScoped<IHrmRewardDisciplineService, HrmRewardDisciplineService>();
        services.AddScoped<IHrmOffboardingService, HrmOffboardingService>();
        services.AddScoped<IHrmDashboardService, HrmDashboardService>();
        return services;
    }

    /// <summary>LMS — cắt khi khách không mua.</summary>
    public static IServiceCollection AddLmsModule(this IServiceCollection services)
    {
        services.AddScoped<ILmsClassService, LmsClassService>();
        services.AddScoped<ILmsCourseService, LmsCourseService>();
        services.AddScoped<ILmsExamService, LmsExamService>();
        services.AddScoped<ILmsInstructorService, LmsInstructorService>();
        services.AddScoped<ILmsReportService, LmsReportService>();
        return services;
    }

    /// <summary>CRM — cắt khi khách không mua.</summary>
    public static IServiceCollection AddCrmModule(this IServiceCollection services)
    {
        services.AddScoped<ICrmCustomerService, CrmCustomerService>();
        services.AddScoped<ICrmSalesService, CrmSalesService>();
        services.AddScoped<ICrmLeadService, CrmLeadService>();
        services.AddScoped<ICrmCampaignService, CrmCampaignService>();
        services.AddScoped<ICrmPromotionService, CrmPromotionService>();
        return services;
    }

    /// <summary>POS — cắt khi khách không mua.</summary>
    public static IServiceCollection AddPosModule(this IServiceCollection services)
    {
        services.AddScoped<IPosConfigService, PosConfigService>();
        services.AddScoped<IPosSalesService, PosSalesService>();
        services.AddScoped<IPosPromoService, PosPromoService>();
        services.AddScoped<IPosReportService, PosReportService>();
        return services;
    }

    /// <summary>PUR — cắt khi khách không mua.</summary>
    public static IServiceCollection AddPurModule(this IServiceCollection services)
    {
        services.AddScoped<IPurPurchasingService, PurPurchasingService>();
        services.AddScoped<IPurReceivingService, PurReceivingService>();
        services.AddScoped<IPurReportService, PurReportService>();
        return services;
    }

    /// <summary>INV — cắt khi khách không mua.</summary>
    public static IServiceCollection AddInvModule(this IServiceCollection services)
    {
        services.AddScoped<IInvMasterService, InvMasterService>();
        services.AddScoped<IInvStockService, InvStockService>();
        services.AddScoped<IInvReportService, InvReportService>();
        return services;
    }

    /// <summary>LOG — cắt khi khách không mua.</summary>
    public static IServiceCollection AddLogModule(this IServiceCollection services)
    {
        services.AddScoped<ILogLogisticsService, LogLogisticsService>();
        services.AddScoped<ILogCodService, LogCodService>();
        services.AddScoped<ILogReturnService, LogReturnService>();
        return services;
    }

    /// <summary>MFG — cắt khi khách không mua.</summary>
    public static IServiceCollection AddMfgModule(this IServiceCollection services)
    {
        services.AddScoped<IMfgProductionService, MfgProductionService>();
        services.AddScoped<IMfgReportService, MfgReportService>();
        return services;
    }

    /// <summary>FSM — cắt khi khách không mua.</summary>
    public static IServiceCollection AddFsmModule(this IServiceCollection services)
    {
        services.AddScoped<IFsmFieldService, FsmFieldService>();
        services.AddScoped<IFsmPartsStockService, FsmPartsStockService>();
        services.AddScoped<IFsmReportService, FsmReportService>();
        return services;
    }

    /// <summary>PJM — cắt khi khách không mua.</summary>
    public static IServiceCollection AddPjmModule(this IServiceCollection services)
    {
        services.AddScoped<IPjmProjectService, PjmProjectService>();
        services.AddScoped<IPjmCostCloseService, PjmCostCloseService>();
        services.AddScoped<IPjmReportService, PjmReportService>();
        return services;
    }

    /// <summary>FIN — cắt khi khách không mua.</summary>
    public static IServiceCollection AddFinModule(this IServiceCollection services)
    {
        services.AddScoped<IFinAccountingService, FinAccountingService>();
        services.AddScoped<IFinCashService, FinCashService>();
        services.AddScoped<IFinBankService, FinBankService>();
        services.AddScoped<IFinApService, FinApService>();
        services.AddScoped<IFinArService, FinArService>();
        services.AddScoped<IFinVatService, FinVatService>();
        services.AddScoped<IFinRevenueService, FinRevenueService>();
        return services;
    }

    /// <summary>AST — cắt khi khách không mua.</summary>
    public static IServiceCollection AddAstModule(this IServiceCollection services)
    {
        services.AddScoped<IAstAssetService, AstAssetService>();
        services.AddScoped<IAstMovementService, AstMovementService>();
        services.AddScoped<IAstStocktakeService, AstStocktakeService>();
        services.AddScoped<IAstReportService, AstReportService>();
        return services;
    }

    /// <summary>BI — cắt khi khách không mua.</summary>
    public static IServiceCollection AddBiModule(this IServiceCollection services)
    {
        services.AddScoped<IBiAnalyticsService, BiAnalyticsService>();
        return services;
    }

    /// <summary>PRT — cắt khi khách không mua.</summary>
    public static IServiceCollection AddPrtModule(this IServiceCollection services)
    {
        services.AddScoped<IPrtPortalService, PrtPortalService>();
        services.AddScoped<IPrtPackageService, PrtPackageService>();
        return services;
    }

    /// <summary>WF — cắt khi không mua; không cắt nếu còn HRM (depends_on).</summary>
    public static IServiceCollection AddWfModule(this IServiceCollection services)
    {
        services.AddScoped<IWfRuntimeService, WfRuntimeService>();
        services.AddScoped<IWorkOpsService, WorkOpsService>();
        return services;
    }
}
