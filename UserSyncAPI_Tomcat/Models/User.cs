using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.ComponentModel.DataAnnotations;

namespace UserSyncAPI_Tomcat.Models
{
    public class UserList
    {
        public List<User>? Users { get; set; }
        public int UsersCount { get; set; }
    }

    public class NewUser {
        public string Database { get; set; }
        public int NewUserId { get; set; }
    }
    public class User
    {
        [Required, StringLength(100)]
        public string SourceSystem { get; set; } // nvarchar(100)
        public List<string> FactoryList { get; set; }
        [Key]
        public int UserId { get; set; } // [int] IDENTITY

        [Required, StringLength(20)]
        public string UserName { get; set; } // char(20)

        [Required, StringLength(100)]
        public string UserLoginName { get; set; } // nvarchar(100)

        [Required, StringLength(100)]
        public string UserLoggedOnAt { get; set; } // char(100)

        [Required, StringLength(30)]
        public string UserFullName { get; set; } // char(30)

        [Required, StringLength(50), EmailAddress]
        public string UserEmail { get; set; } // char(50)

        [Required, StringLength(4)]
        public string UserInitials { get; set; } // char(4)

        [Required, StringLength(50)]
        public string UserPassword { get; set; } // nvarchar(50)

        [Required]
        public int UserDepartment { get; set; } // int

        [Required]
        public bool UserLoggedIn { get; set; } // bit

        [Required]
        public int UserInModule { get; set; } // int

        [StringLength(20)]
        public string G2Version { get; set; } // nchar(20), nullable

        public DateTime? LastLogin { get; set; } // datetime, nullable

        [Required, StringLength(100)]
        public string OS { get; set; } // char(100)

        [Required, StringLength(50)]
        public string CLR { get; set; } // char(50)

        [Required, StringLength(400)]
        public string UserMenus { get; set; } // char(400)

        [Required]
        public int LoginCount { get; set; } // int

        [Required]
        public bool LibrariesReadOnly { get; set; } // bit

        [Required]
        public int GroupCompany { get; set; } // int

        [Required, StringLength(30)]
        public string Screen1Res { get; set; } // nchar(30)

        [Required, StringLength(30)]
        public string Screen2Res { get; set; }

        [Required, StringLength(30)]
        public string Screen3Res { get; set; }

        [Required, StringLength(30)]
        public string Screen4Res { get; set; }

        [Required]
        public int POAuthId { get; set; }

        [Required]
        public bool POAuthAll { get; set; }

        [Required]
        public bool OrderAlerts { get; set; }

        [Required]
        public int POAuthTempUserId { get; set; }

        [Required]
        public int MaxOrderValue { get; set; }

        [Required]
        public bool OutOfOffice { get; set; }

        [Required]
        public int DefaultOrderDepartment { get; set; }

        [Required]
        public int PORoleId { get; set; }

        [Required]
        public bool Deleted { get; set; }

        [StringLength(10)]
        public string AliasUserName1 { get; set; } // nchar(10), nullable

        [Required, StringLength(50)]
        public string InvoiceBarcodePrinter { get; set; }

        [Required, StringLength(50)]
        public string SmtpServer { get; set; }

        [Required]
        public int FactoryId { get; set; }

        [Required]
        public int PieceMonitoringAccessLevel { get; set; }

        [Required]
        public int ExClassEdit { get; set; }

        [Required]
        public int TimesheetsAccessLevel { get; set; }

        [Required, StringLength(50)]
        public string InitialWindows { get; set; }

        [Required]
        public int FabScheduleAccessLevel { get; set; }

        [Required]
        public int FabLineScheduleAccessLevel { get; set; }

        [Required]
        public int PaintLineScheduleAccessLevel { get; set; }

        [Required]
        public int ContractsAccessLevel { get; set; }

        [StringLength(20)]
        public string G2UpdaterVersion { get; set; } // nvarchar(20), nullable

        [Required]
        public int UpdateLocationId { get; set; }

        [Required]
        public bool AllocationAdmin { get; set; }

        public DateTime? UserPasswordLastChanged { get; set; } // date, nullable

        public DateTime? DateCreated { get; set; } // date, nullable

        public int? CreatedByUserId { get; set; }

        public DateTime? DateModified { get; set; }

        public int? ModifiedByUserId { get; set; }

        [Required, StringLength(100)]
        public string LoggedInOnComputer { get; set; }

        [StringLength(400)]
        public string YLoc { get; set; }

        [StringLength(400)]
        public string YLocDsc { get; set; }

        public int? Locked { get; set; }

        public int? FAttempt { get; set; }

        [StringLength(50)]
        public string Remarks { get; set; }

        public DateTime? ReleaseDt { get; set; }

        [StringLength(50)]
        public string ReleaseBy { get; set; }

        public int? Inactive { get; set; }

        [StringLength(50)]
        public string InactiveRemarks { get; set; }

        public DateTime? InactiveReleaseDt { get; set; }

        [StringLength(50)]
        public string InactiveReleaseBy { get; set; }

        [StringLength(50)]
        public string PasswordAttempts { get; set; }

        [StringLength(50)]
        public string PasswordUpdatedFlag { get; set; }

        public DateTime? UnlockDate { get; set; }
    }
}