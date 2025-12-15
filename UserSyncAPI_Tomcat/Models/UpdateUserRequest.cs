using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserSyncAPI_Tomcat.Models
{
    public class UpdateUserRequest
    {
        [Required, StringLength(100)]
        public required string SourceSystem { get; set; } // nvarchar(100)
        /// <summary>
        /// List of DB keys from Web.config appSettings
        /// </summary>
        [Required]
        public required List<string> TargetFactories { get; set; }

        [Key]
        public int UserId { get; set; } // [int] IDENTITY

        [StringLength(20)]
        public string? UserName { get; set; } // char(20)

        [StringLength(100)]
        public string? UserLoginName { get; set; } // nvarchar(100)

        [StringLength(100)]
        public string? UserLoggedOnAt { get; set; } // char(100)

        [StringLength(30)]
        public string? UserFullName { get; set; } // char(30)

        [StringLength(50), EmailAddress]
        public string? UserEmail { get; set; } // char(50)

        [StringLength(4)]
        public string? UserInitials { get; set; } // char(4)

        public int? UserDepartment { get; set; } // int

        public bool? UserLoggedIn { get; set; } // bit

        public int? UserInModule { get; set; } // int

        [StringLength(20)]
        public string? G2Version { get; set; } // nchar(20), nullable

        public DateTime? LastLogin { get; set; } // datetime, nullable

        [StringLength(100)]
        public string? OS { get; set; } // char(100)

        [StringLength(50)]
        public string? CLR { get; set; } // char(50)

        [StringLength(400)]
        public string? UserMenus { get; set; } // char(400)


        public int? LoginCount { get; set; } // int

        public bool? LibrariesReadOnly { get; set; } // bit

        public int? GroupCompany { get; set; } // int

        [StringLength(30)]
        public string? Screen1Res { get; set; } // nchar(30)

        [StringLength(30)]
        public string? Screen2Res { get; set; }

        [StringLength(30)]
        public string? Screen3Res { get; set; }

        [StringLength(30)]
        public string? Screen4Res { get; set; }

        public int? POAuthId { get; set; }

        public bool? POAuthAll { get; set; }

        public bool? OrderAlerts { get; set; }

        public int? POAuthTempUserId { get; set; }

        public int? MaxOrderValue { get; set; }

        public bool? OutOfOffice { get; set; }

        public int? DefaultOrderDepartment { get; set; }

        public int? PORoleId { get; set; }

        public bool? Deleted { get; set; }

        [StringLength(10)]
        public string? AliasUserName1 { get; set; } // nchar(10), nullable

        [StringLength(50)]
        public string? InvoiceBarcodePrinter { get; set; }

        [StringLength(50)]
        public string? SmtpServer { get; set; }

        public int? FactoryId { get; set; }

        public int? PieceMonitoringAccessLevel { get; set; }

        public int? ExClassEdit { get; set; }

        public int? TimesheetsAccessLevel { get; set; }

        [StringLength(50)]
        public string? InitialWindows { get; set; }

        public int? FabScheduleAccessLevel { get; set; }

        public int? FabLineScheduleAccessLevel { get; set; }

        public int? PaintLineScheduleAccessLevel { get; set; }

        public int? ContractsAccessLevel { get; set; }

        [StringLength(20)]
        public string? G2UpdaterVersion { get; set; } // nvarchar(20), nullable

        public int? UpdateLocationId { get; set; }

        public bool? AllocationAdmin { get; set; }

        public DateTime? UserPasswordLastChanged { get; set; } // date, nullable

        [Required]
        public required int ModifiedByUserId { get; set; }

        [StringLength(100)]
        public string? LoggedInOnComputer { get; set; }

        [StringLength(400)]
        public string? YLoc { get; set; }

        [StringLength(400)]
        public string? YLocDsc { get; set; }

        public int? Locked { get; set; }

        public int? FAttempt { get; set; }

        [StringLength(50)]
        public string? Remarks { get; set; }

        public DateTime? ReleaseDt { get; set; }

        [StringLength(50)]
        public string? ReleaseBy { get; set; }

        public int? Inactive { get; set; }

        [StringLength(50)]
        public string? InactiveRemarks { get; set; }

        public DateTime? InactiveReleaseDt { get; set; }

        [StringLength(50)]
        public string? InactiveReleaseBy { get; set; }

        [StringLength(50)]
        public string? PasswordAttempts { get; set; }

        [StringLength(50)]
        public string? PasswordUpdatedFlag { get; set; }

        public DateTime? UnlockDate { get; set; }
    }
}