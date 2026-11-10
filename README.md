# Smart Diabetes Health Platform

## اسم المشروع
منصة ذكية لإدارة ومتابعة الحالة الصحية لمرضى السكري

## التقنية المعتمدة
- Frontend: Flutter + Dart
- Patient UI + Doctor UI: داخل تطبيق Flutter واحد مع Role-Based UI
- Backend: ASP.NET Core Web API + C#
- Database: Microsoft SQL Server
- Query Language: SQL
- ORM: Entity Framework Core
- Authentication: ASP.NET Core Identity + JWT
- Notifications: Firebase Cloud Messaging
- API Testing: Postman
- UI/UX: Figma
- Version Control: Git + GitHub

## البنية البرمجية المعتمدة

```text
DiabetesPlatform/
│
├── API/
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
│
├── Application/
│   ├── DTOs/
│   ├── Services/
│   ├── Interfaces/
│   └── Validators/
│
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Rules/
│
├── Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   ├── Authentication/
│   └── Notifications/
│
└── Tests/
```

## ملاحظات معتمدة
- Alert هو حدث/حالة تنتج من قواعد التحليل، بينما Notification هي عملية إيصال الرسالة للمستخدم.
- التحليلات وقواعد اكتشاف القراءات غير الطبيعية تنفذ في Backend وليس داخل Flutter.
- Health Stability Indicator هو مؤشر برمجي خاص بالمشروع وليس تشخيصاً طبياً مستقلاً.
- يجب على Backend تطبيق صلاحيات الوصول بحيث لا يستطيع الطبيب قراءة بيانات مريض غير مرتبط به.
- لا يتم اتخاذ حكم على استقرار الحالة من قراءة سكر واحدة؛ يعتمد التحليل على فترة زمنية وبيانات كافية.
- الإصدار الأول يعتمد على Rule-Based Analytics وTrend Analysis ولا يفترض استخدام Machine Learning.

## حالة التنفيذ
هذا الفرع مخصص لتأسيس المعمارية النهائية قبل بدء تنفيذ الميزات. سيتم تنفيذ قاعدة البيانات، Identity/JWT، APIs، ثم Flutter Patient/Doctor UI بشكل تدريجي مع الاختبارات.
