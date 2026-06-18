namespace BalochiAcademy.Domain.Enums;

public enum QuestionKind   { Objective, Subjective }
public enum Difficulty     { Easy, Medium, Hard }
public enum TestKind       { Objective, Subjective, Daily, Monthly, Challenge, Weekly, Parent }
public enum ComplaintStatus { Open, InProgress, Resolved, Closed }
public enum NotificationType { Info, Warning, Success, Alert }
public enum WithdrawalStatus { Pending, Approved, Rejected, Paid }
public enum PayoutService  { JazzCash, EasyPaisa, BankTransfer }
public enum ContentKind    { Video, Keynote, Resource, Ebook, PastPaper, Simulation, VirtualLab }
public enum LoginStatus    { Ok, Logout, Failed }
public enum AuditAction    { Create, Update, Delete, Login, Logout, Publish }
