import 'package:material_ui/material_ui.dart';
import 'package:go_router/go_router.dart';

class WebLandingPage extends StatelessWidget {
  const WebLandingPage({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topRight,
            end: Alignment.bottomLeft,
            colors: [
              theme.colorScheme.primaryContainer,
              theme.scaffoldBackgroundColor,
              Colors.white,
            ],
          ),
        ),
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 1180),
                child: Column(
                  children: [
                    _Header(onLogin: () => context.go('/login')),
                    const SizedBox(height: 72),
                    LayoutBuilder(
                      builder: (context, constraints) {
                        final wide = constraints.maxWidth >= 820;
                        final hero = _HeroContent(
                          onLogin: () => context.go('/login'),
                          onRegister: () => context.go('/register'),
                        );

                        return wide
                            ? Row(
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: [
                                  Expanded(flex: 6, child: hero),
                                  const SizedBox(width: 24),
                                  const Expanded(flex: 5, child: _DashboardPreview()),
                                ],
                              )
                            : Column(
                                children: [
                                  hero,
                                  const SizedBox(height: 36),
                                  const _DashboardPreview(),
                                ],
                              );
                      },
                    ),
                    const SizedBox(height: 72),
                    const _FeatureSection(),
                    const SizedBox(height: 28),
                    Text(
                      'المريض • الطبيب • مدير النظام — منصة واحدة وبيانات مترابطة',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _Header extends StatelessWidget {
  const _Header({required this.onLogin});

  final VoidCallback onLogin;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      children: [
        Container(
          width: 52,
          height: 52,
          decoration: BoxDecoration(
            color: theme.colorScheme.primary,
            borderRadius: BorderRadius.circular(16),
          ),
          child: const Icon(
            Icons.monitor_heart_outlined,
            color: Colors.white,
            size: 30,
          ),
        ),
        const SizedBox(width: 14),
        const Expanded(
          child: Text(
            'Smart Diabetes\nHealth Platform',
            style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
          ),
        ),
        OutlinedButton(
          onPressed: onLogin,
          child: const Text('تسجيل الدخول'),
        ),
      ],
    );
  }
}

class _HeroContent extends StatelessWidget {
  const _HeroContent({
    required this.onLogin,
    required this.onRegister,
  });

  final VoidCallback onLogin;
  final VoidCallback onRegister;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.all(12),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
            decoration: BoxDecoration(
              color: theme.colorScheme.primary.withValues(alpha: 0.10),
              borderRadius: BorderRadius.circular(30),
            ),
            child: Text(
              'صحة أفضل • متابعة أذكى',
              style: TextStyle(
                color: theme.colorScheme.primary,
                fontWeight: FontWeight.w700,
              ),
            ),
          ),
          const SizedBox(height: 22),
          Text(
            'منصة واحدة لمتابعة\nصحة مرضى السكري',
            style: theme.textTheme.displaySmall?.copyWith(
              fontWeight: FontWeight.w900,
              height: 1.15,
            ),
          ),
          const SizedBox(height: 18),
          Text(
            'سجّل قراءات السكر، تابع التحليلات، نظّم الأدوية، واستفد من المتابعة الطبية والإشعارات ضمن نظام واحد مرتبط بنفس بيانات التطبيق.',
            style: theme.textTheme.titleMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
              height: 1.6,
            ),
          ),
          const SizedBox(height: 28),
          Wrap(
            spacing: 12,
            runSpacing: 12,
            children: [
              FilledButton.icon(
                onPressed: onLogin,
                icon: const Icon(Icons.login),
                label: const Text('ابدأ الآن'),
              ),
              OutlinedButton.icon(
                onPressed: onRegister,
                icon: const Icon(Icons.person_add_alt_1),
                label: const Text('إنشاء حساب'),
              ),
            ],
          ),
          const SizedBox(height: 28),
          const Wrap(
            spacing: 18,
            runSpacing: 12,
            children: [
              _TrustPoint(icon: Icons.verified_user_outlined, text: 'JWT وصلاحيات حسب الدور'),
              _TrustPoint(icon: Icons.analytics_outlined, text: 'تحليلات السكر في Backend'),
              _TrustPoint(icon: Icons.notifications_active_outlined, text: 'تنبيهات وإشعارات'),
            ],
          ),
        ],
      ),
    );
  }
}

class _TrustPoint extends StatelessWidget {
  const _TrustPoint({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 20, color: Theme.of(context).colorScheme.primary),
        const SizedBox(width: 7),
        Text(text),
      ],
    );
  }
}

class _DashboardPreview extends StatelessWidget {
  const _DashboardPreview();
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      elevation: 8,
      shadowColor: theme.colorScheme.primary.withValues(alpha: 0.15),
      clipBehavior: Clip.antiAlias,
      child: Container(
        padding: const EdgeInsets.all(22),
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: [Color(0xFFEFFBFC), Colors.white],
            begin: Alignment.topRight,
            end: Alignment.bottomLeft,
          ),
        ),
        child: Column(
          children: [
            Row(
              children: [
                CircleAvatar(
                  backgroundColor: theme.colorScheme.primary,
                  child: const Icon(Icons.person, color: Colors.white),
                ),
                const SizedBox(width: 12),
                const Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('مرحباً أحمد', style: TextStyle(fontWeight: FontWeight.bold)),
                      Text('لوحة المريض'),
                    ],
                  ),
                ),
                Icon(Icons.notifications_none, color: theme.colorScheme.primary),
              ],
            ),
            const SizedBox(height: 22),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(22),
                border: Border.all(color: theme.colorScheme.primary.withValues(alpha: 0.12)),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text('آخر قراءة للسكر'),
                  const SizedBox(height: 8),
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Text(
                        '125',
                        style: theme.textTheme.displaySmall?.copyWith(
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      const Padding(
                        padding: EdgeInsets.only(bottom: 8, right: 6),
                        child: Text('mg/dL'),
                      ),
                    ],
                  ),
                  const SizedBox(height: 10),
                  Chip(
                    avatar: const Icon(Icons.check_circle, size: 18),
                    label: const Text('ضمن النطاق'),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
            Row(
              children: const [
                Expanded(child: _PreviewMetric(title: 'المتوسط', value: '132')),
                SizedBox(width: 10),
                Expanded(child: _PreviewMetric(title: 'ضمن النطاق', value: '82%')),
                SizedBox(width: 10),
                Expanded(child: _PreviewMetric(title: 'القراءات', value: '24')),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _PreviewMetric extends StatelessWidget {
  const _PreviewMetric({required this.title, required this.value});

  final String title;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        children: [
          Text(title, textAlign: TextAlign.center),
          const SizedBox(height: 6),
          Text(value, style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 20)),
        ],
      ),
    );
  }
}

class _FeatureSection extends StatelessWidget {
  const _FeatureSection();

  @override
  Widget build(BuildContext context) {
    const features = [
      (Icons.monitor_heart_outlined, 'قراءات السكر', 'إضافة ومراجعة وتحليل القراءات.'),
      (Icons.medication_outlined, 'الأدوية', 'تنظيم الأدوية والجداول والسجلات.'),
      (Icons.medical_services_outlined, 'متابعة الطبيب', 'الطبيب يتابع المرضى المرتبطين به فقط.'),
      (Icons.admin_panel_settings_outlined, 'إدارة النظام', 'اعتماد الأطباء والمستخدمون والسجلات.'),
    ];

    return Column(
      children: [
        Text(
          'مزايا المنصة',
          style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 18),
        LayoutBuilder(
          builder: (context, constraints) {
            final columns = constraints.maxWidth >= 900 ? 4 : 2;
            return GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: features.length,
              gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: columns,
                mainAxisExtent: 150,
                crossAxisSpacing: 14,
                mainAxisSpacing: 14,
              ),
              itemBuilder: (context, index) {
                final item = features[index];
                return Card(
                  child: Padding(
                    padding: const EdgeInsets.all(18),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(item.$1, size: 32, color: Theme.of(context).colorScheme.primary),
                        const SizedBox(height: 10),
                        Text(item.$2, style: const TextStyle(fontWeight: FontWeight.bold)),
                        const SizedBox(height: 6),
                        Text(item.$3, textAlign: TextAlign.center),
                      ],
                    ),
                  ),
                );
              },
            );
          },
        ),
      ],
    );
  }
}
