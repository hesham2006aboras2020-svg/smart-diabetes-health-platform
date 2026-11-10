# UI/UX Direction

The application will use a clean, modern healthcare interface with Arabic-first support and clear visual hierarchy.

## Writing
- Use short, reassuring, human-readable labels.
- Prefer action-oriented button text such as "تسجيل الدخول", "إنشاء حساب", "حفظ التغييرات", "إرسال طلب الاعتماد", "عرض التفاصيل".
- Validation messages should explain the problem and the next action.
- Avoid technical wording in patient-facing screens.
- Doctor approval status should be explicit: "بانتظار موافقة الإدارة", "تم اعتماد الحساب", "تم رفض الطلب", "الحساب موقوف".

## Buttons
- Primary action: one clear primary button per screen.
- Secondary actions should be visually quieter.
- Destructive actions such as reject, suspend, or delete require confirmation.
- Loading states must disable duplicate submissions and show clear progress.

## Forms
- Clear labels above inputs.
- Helpful examples/placeholders where appropriate.
- Inline validation.
- Password fields include show/hide control and password requirements.
- Doctor registration clearly separates professional information from account information.

## Patient experience
- Dashboard focuses on today's status, latest glucose reading, trend, medications, and important alerts.
- Avoid alarming language for analytics; alerts explain what happened and what the user can do.
- Health Stability Indicator is presented as a project indicator, not as a diagnosis.

## Doctor experience
- Dashboard emphasizes assigned patients, recent alerts, trends, and follow-up priority.
- Patient information is grouped into readable cards and charts.
- Notes and recommendations use a clear timeline/history presentation.

## Admin experience
- Pending doctor approvals are prominent.
- Approval/rejection actions show confirmation and optional notes/reason.
- Audit-sensitive actions should provide visible feedback after completion.

## Visual principles
- Consistent spacing, typography, iconography, and component states.
- Accessible contrast and readable text.
- Empty, loading, success, warning, and error states are designed intentionally.
- Responsive layouts should remain usable on common mobile screen sizes.
