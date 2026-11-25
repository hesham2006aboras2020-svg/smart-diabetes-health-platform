import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'auth_controller.dart';

class RegisterPage extends ConsumerStatefulWidget {
  const RegisterPage({super.key});

  @override
  ConsumerState<RegisterPage> createState() => _RegisterPageState();
}

class _RegisterPageState extends ConsumerState<RegisterPage> {
  final _formKey = GlobalKey<FormState>();
  final _fullNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _licenseController = TextEditingController();
  final _specializationController = TextEditingController();
  final _clinicController = TextEditingController();

  String _role = 'Patient';
  bool _obscurePassword = true;
  bool _submitting = false;

  @override
  void dispose() {
    _fullNameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _licenseController.dispose();
    _specializationController.dispose();
    _clinicController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authControllerProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('إنشاء حساب')),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 520),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      'ابدأ باستخدام منصة السكري الذكية',
                      style: Theme.of(context).textTheme.headlineSmall,
                    ),
                    const SizedBox(height: 8),
                    const Text(
                      'اختر نوع الحساب وأدخل بياناتك الأساسية.',
                    ),
                    const SizedBox(height: 24),
                    SegmentedButton<String>(
                      segments: const [
                        ButtonSegment(
                          value: 'Patient',
                          label: Text('مريض'),
                          icon: Icon(Icons.person_outline),
                        ),
                        ButtonSegment(
                          value: 'Doctor',
                          label: Text('طبيب'),
                          icon: Icon(Icons.medical_services_outlined),
                        ),
                      ],
                      selected: {_role},
                      onSelectionChanged: _submitting
                          ? null
                          : (selection) =>
                              setState(() => _role = selection.first),
                    ),
                    const SizedBox(height: 20),
                    TextFormField(
                      controller: _fullNameController,
                      minLines: 1,
                      maxLines: 1,
                      textInputAction: TextInputAction.next,
                      decoration: const InputDecoration(
                        isDense: true,
                        contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                        labelText: 'الاسم الكامل',
                        prefixIcon: Icon(Icons.badge_outlined),
                      ),
                      validator: (value) =>
                          value == null || value.trim().isEmpty
                              ? 'أدخل الاسم الكامل.'
                              : null,
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _emailController,
                      minLines: 1,
                      maxLines: 1,
                      keyboardType: TextInputType.emailAddress,
                      textDirection: TextDirection.ltr,
                      textInputAction: TextInputAction.next,
                      decoration: const InputDecoration(
                        isDense: true,
                        contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                        labelText: 'البريد الإلكتروني',
                        prefixIcon: Icon(Icons.email_outlined),
                      ),
                      validator: (value) {
                        final email = value?.trim() ?? '';
                        if (email.isEmpty) return 'أدخل البريد الإلكتروني.';
                        if (!email.contains('@') || !email.contains('.')) {
                          return 'أدخل بريدًا إلكترونيًا صحيحًا.';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _passwordController,
                      minLines: 1,
                      maxLines: 1,
                      obscureText: _obscurePassword,
                      textInputAction: TextInputAction.next,
                      decoration: InputDecoration(
                        isDense: true,
                        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                        labelText: 'كلمة المرور',
                        prefixIcon: const Icon(Icons.lock_outline),
                        suffixIcon: IconButton(
                          tooltip: _obscurePassword
                              ? 'إظهار كلمة المرور'
                              : 'إخفاء كلمة المرور',
                          onPressed: () => setState(
                            () => _obscurePassword = !_obscurePassword,
                          ),
                          icon: Icon(
                            _obscurePassword
                                ? Icons.visibility_outlined
                                : Icons.visibility_off_outlined,
                          ),
                        ),
                      ),
                      validator: (value) {
                        if (value == null || value.isEmpty) {
                          return 'أدخل كلمة المرور.';
                        }
                        if (value.length < 8) {
                          return 'كلمة المرور يجب ألا تقل عن 8 أحرف.';
                        }
                        if (!RegExp(r'[A-Z]').hasMatch(value) ||
                            !RegExp(r'[a-z]').hasMatch(value) ||
                            !RegExp(r'[0-9]').hasMatch(value)) {
                          return 'استخدم حرفًا كبيرًا وصغيرًا ورقمًا.';
                        }
                        return null;
                      },
                    ),
                    if (_role == 'Doctor') ...[
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _licenseController,
                        minLines: 1,
                        maxLines: 1,
                        textInputAction: TextInputAction.next,
                        decoration: const InputDecoration(
                        isDense: true,
                        contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                          labelText: 'رقم الترخيص الطبي',
                          prefixIcon: Icon(Icons.verified_user_outlined),
                        ),
                        validator: (value) =>
                            _role == 'Doctor' &&
                                    (value == null || value.trim().isEmpty)
                                ? 'رقم الترخيص الطبي مطلوب للطبيب.'
                                : null,
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _specializationController,
                        minLines: 1,
                        maxLines: 1,
                        textInputAction: TextInputAction.next,
                        decoration: const InputDecoration(
                        isDense: true,
                        contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                          labelText: 'التخصص الطبي (اختياري)',
                          prefixIcon: Icon(Icons.local_hospital_outlined),
                        ),
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _clinicController,
                        minLines: 1,
                        maxLines: 1,
                        textInputAction: TextInputAction.done,
                        decoration: const InputDecoration(
                        isDense: true,
                        contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                          labelText: 'اسم العيادة (اختياري)',
                          prefixIcon: Icon(Icons.business_outlined),
                        ),
                      ),
                    ],
                    if (auth.message != null &&
                        auth.status == AuthStatus.error) ...[
                      const SizedBox(height: 16),
                      Text(
                        auth.message!,
                        style: TextStyle(
                          color: Theme.of(context).colorScheme.error,
                        ),
                      ),
                    ],
                    const SizedBox(height: 24),
                    FilledButton(
                      onPressed: _submitting ? null : _submit,
                      child: Padding(
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        child: _submitting
                            ? const SizedBox(
                                width: 22,
                                height: 22,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                ),
                              )
                            : const Text('إنشاء الحساب'),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextButton(
                      onPressed: _submitting
                          ? null
                          : () => Navigator.of(context).pop(),
                      child: const Text('لديك حساب؟ تسجيل الدخول'),
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

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _submitting = true);
    final response = await ref.read(authControllerProvider.notifier).register(
          fullName: _fullNameController.text,
          email: _emailController.text,
          password: _passwordController.text,
          role: _role,
          licenseNumber:
              _role == 'Doctor' ? _licenseController.text : null,
          specialization:
              _role == 'Doctor' ? _specializationController.text : null,
          clinicName: _role == 'Doctor' ? _clinicController.text : null,
        );
    if (!mounted) return;
    setState(() => _submitting = false);

    if (response == null) return;

    if (response.requiresApproval) {
      await showDialog<void>(
        context: context,
        builder: (context) => AlertDialog(
          title: const Text('تم إرسال طلبك'),
          content: const Text(
            'تم إنشاء حساب الطبيب بنجاح، وسيصبح تسجيل الدخول متاحًا بعد موافقة الإدارة.',
          ),
          actions: [
            FilledButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('حسنًا'),
            ),
          ],
        ),
      );
      if (mounted) Navigator.of(context).pop();
      return;
    }

    if (response.authentication != null && mounted) {
      Navigator.of(context).pop();
    }
  }
}
