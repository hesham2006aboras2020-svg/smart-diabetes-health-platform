import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

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
  final _phoneController = TextEditingController();
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
    _phoneController.dispose();
    _passwordController.dispose();
    _licenseController.dispose();
    _specializationController.dispose();
    _clinicController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final auth = ref.watch(authControllerProvider);

    return Scaffold(
      body: DecoratedBox(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topRight,
            end: Alignment.bottomLeft,
            colors: [
              theme.colorScheme.primaryContainer.withValues(alpha: 0.7),
              theme.scaffoldBackgroundColor,
              Colors.white,
            ],
          ),
        ),
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 560),
                child: Card(
                  clipBehavior: Clip.antiAlias,
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(28, 28, 28, 26),
                    child: Form(
                      key: _formKey,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          Row(
                            children: [
                              IconButton(
                                tooltip: 'رجوع',
                                onPressed: _submitting ? null : () => context.pop(),
                                icon: const Icon(Icons.arrow_forward),
                              ),
                              const SizedBox(width: 4),
                              Expanded(
                                child: Text(
                                  'إنشاء حساب جديد',
                                  style: theme.textTheme.headlineSmall?.copyWith(
                                    fontWeight: FontWeight.w800,
                                  ),
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 6),
                          Text(
                            'أنشئ حسابك وابدأ متابعة صحتك من مكان واحد.',
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: theme.colorScheme.onSurfaceVariant,
                              height: 1.5,
                            ),
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
                          _field(
                            controller: _fullNameController,
                            label: 'الاسم الكامل',
                            icon: Icons.badge_outlined,
                            textInputAction: TextInputAction.next,
                            validator: (value) =>
                                value == null || value.trim().isEmpty
                                    ? 'أدخل الاسم الكامل.'
                                    : null,
                          ),
                          const SizedBox(height: 14),
                          _field(
                            controller: _phoneController,
                            label: 'رقم الهاتف',
                            hint: '+967 7XXXXXXXX',
                            icon: Icons.phone_outlined,
                            keyboardType: TextInputType.phone,
                            direction: TextDirection.ltr,
                            textInputAction: TextInputAction.next,
                            validator: (value) {
                              final phone = value?.trim() ?? '';
                              final normalized =
                                  phone.replaceAll(RegExp(r'[^+0-9]'), '');
                              if (normalized.isEmpty) {
                                return 'أدخل رقم الهاتف.';
                              }
                              if (!RegExp(r'^\+?[1-9]\d{7,14}$')
                                  .hasMatch(normalized)) {
                                return 'أدخل رقم هاتف صحيحًا بصيغة دولية.';
                              }
                              return null;
                            },
                          ),
                          const SizedBox(height: 14),
                          _field(
                            controller: _emailController,
                            label: 'البريد الإلكتروني',
                            icon: Icons.email_outlined,
                            keyboardType: TextInputType.emailAddress,
                            direction: TextDirection.ltr,
                            textInputAction: TextInputAction.next,
                            validator: (value) {
                              final email = value?.trim() ?? '';
                              if (email.isEmpty) return 'أدخل البريد الإلكتروني.';
                              if (!email.contains('@') || !email.contains('.')) {
                                return 'أدخل بريدًا إلكترونيًا صحيحًا.';
                              }
                              return null;
                            },
                          ),
                          const SizedBox(height: 14),
                          TextFormField(
                            controller: _passwordController,
                            minLines: 1,
                            maxLines: 1,
                            obscureText: _obscurePassword,
                            keyboardType: TextInputType.number,
                            textInputAction: TextInputAction.next,
                            decoration: InputDecoration(
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
                              if (!RegExp(r'^\d{6,15}$').hasMatch(value)) {
                                return 'كلمة المرور يجب أن تكون من 6 إلى 15 رقمًا فقط.';
                              }
                              return null;
                            },
                          ),
                          if (_role == 'Doctor') ...[
                            const SizedBox(height: 14),
                            _field(
                              controller: _licenseController,
                              label: 'رقم الترخيص الطبي',
                              icon: Icons.verified_user_outlined,
                              textInputAction: TextInputAction.next,
                              validator: (value) =>
                                  value == null || value.trim().isEmpty
                                      ? 'رقم الترخيص الطبي مطلوب للطبيب.'
                                      : null,
                            ),
                            const SizedBox(height: 14),
                            _field(
                              controller: _specializationController,
                              label: 'التخصص الطبي (اختياري)',
                              icon: Icons.local_hospital_outlined,
                              textInputAction: TextInputAction.next,
                            ),
                            const SizedBox(height: 14),
                            _field(
                              controller: _clinicController,
                              label: 'اسم العيادة (اختياري)',
                              icon: Icons.business_outlined,
                              textInputAction: TextInputAction.done,
                            ),
                          ],
                          if (auth.message != null &&
                              auth.status == AuthStatus.error) ...[
                            const SizedBox(height: 14),
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: theme.colorScheme.errorContainer,
                                borderRadius: BorderRadius.circular(14),
                              ),
                              child: Text(
                                auth.message!,
                                style: TextStyle(
                                  color: theme.colorScheme.onErrorContainer,
                                ),
                              ),
                            ),
                          ],
                          const SizedBox(height: 22),
                          FilledButton.icon(
                            onPressed: _submitting ? null : _submit,
                            icon: _submitting
                                ? const SizedBox(
                                    width: 20,
                                    height: 20,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                    ),
                                  )
                                : const Icon(Icons.person_add_alt_1),
                            label: Text(
                              _submitting
                                  ? 'جارٍ إنشاء الحساب...'
                                  : 'إنشاء الحساب',
                            ),
                          ),
                          const SizedBox(height: 8),
                          TextButton(
                            onPressed: _submitting
                                ? null
                                : () => context.go('/login'),
                            child: const Text('لديك حساب؟ تسجيل الدخول'),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _field({
    required TextEditingController controller,
    required String label,
    required IconData icon,
    String? hint,
    TextInputType? keyboardType,
    TextDirection? direction,
    TextInputAction? textInputAction,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      minLines: 1,
      maxLines: 1,
      keyboardType: keyboardType,
      textDirection: direction,
      textInputAction: textInputAction,
      decoration: InputDecoration(
        labelText: label,
        hintText: hint,
        prefixIcon: Icon(icon),
      ),
      validator: validator,
    );
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _submitting = true);
    final response =
        await ref.read(authControllerProvider.notifier).register(
              fullName: _fullNameController.text,
              email: _emailController.text,
              phoneNumber: _phoneController.text,
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
      if (mounted) context.go('/login');
      return;
    }

    if (response.authentication != null && mounted) {
      context.go('/home');
    }
  }
}
