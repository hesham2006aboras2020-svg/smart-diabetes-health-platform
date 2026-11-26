import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';

import 'auth_controller.dart';

final Uri whatsappChatUri =
    Uri.parse('https://wa.me/967739303311');
const String phoneDialDestination = '+697772106560';
final Uri phoneDialUri = Uri(scheme: 'tel', path: phoneDialDestination);

class LoginPage extends ConsumerStatefulWidget {
  const LoginPage({super.key});

  @override
  ConsumerState<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends ConsumerState<LoginPage> {
  final _phoneController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _rememberMe = false;
  bool _obscurePassword = true;

  @override
  void dispose() {
    _phoneController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final auth = ref.watch(authControllerProvider);
    final loading = auth.status == AuthStatus.loading;

    return Scaffold(
      body: DecoratedBox(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topRight,
            end: Alignment.bottomLeft,
            colors: [
              theme.colorScheme.primaryContainer.withValues(alpha: 0.75),
              theme.scaffoldBackgroundColor,
              const Color(0xFF102426),
            ],
          ),
        ),
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 460),
                child: Card(
                  clipBehavior: Clip.antiAlias,
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(28, 30, 28, 26),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Center(
                          child: Container(
                            width: 68,
                            height: 68,
                            decoration: BoxDecoration(
                              color: theme.colorScheme.primary,
                              borderRadius: BorderRadius.circular(22),
                            ),
                            child: const Icon(
                              Icons.monitor_heart_outlined,
                              color: Colors.white,
                              size: 36,
                            ),
                          ),
                        ),
                        const SizedBox(height: 20),
                        Text(
                          'مرحبًا بعودتك',
                          textAlign: TextAlign.center,
                          style: theme.textTheme.headlineSmall?.copyWith(
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                        const SizedBox(height: 8),
                        Text(
                          'سجّل دخولك لمتابعة صحتك وقراءات السكر بسهولة.',
                          textAlign: TextAlign.center,
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                            height: 1.5,
                          ),
                        ),
                        const SizedBox(height: 28),
                        TextField(
                          controller: _phoneController,
                          minLines: 1,
                          maxLines: 1,
                          keyboardType: TextInputType.phone,
                          textDirection: TextDirection.ltr,
                          decoration: const InputDecoration(
                            labelText: 'رقم الهاتف',
                            hintText: '+967 7XXXXXXXX',
                            prefixIcon: Icon(Icons.phone_outlined),
                          ),
                        ),
                        const SizedBox(height: 14),
                        TextField(
                          controller: _passwordController,
                          minLines: 1,
                          maxLines: 1,
                          obscureText: _obscurePassword,
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
                          onSubmitted: (_) => _submit(),
                        ),
                        const SizedBox(height: 8),
                        CheckboxListTile(
                          value: _rememberMe,
                          onChanged: loading
                              ? null
                              : (value) => setState(
                                    () => _rememberMe = value ?? false,
                                  ),
                          title: const Text('تذكرني'),
                          contentPadding: EdgeInsets.zero,
                          controlAffinity: ListTileControlAffinity.leading,
                          dense: true,
                        ),
                        if (auth.message != null) ...[
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
                          onPressed: loading ? null : _submit,
                          icon: loading
                              ? const SizedBox(
                                  width: 20,
                                  height: 20,
                                  child: CircularProgressIndicator(strokeWidth: 2),
                                )
                              : const Icon(Icons.login),
                          label: Text(loading ? 'جارٍ تسجيل الدخول...' : 'تسجيل الدخول'),
                        ),
                        const SizedBox(height: 8),
                        TextButton(
                          onPressed: loading ? null : () => context.push('/register'),
                          child: const Text('ليس لديك حساب؟ إنشاء حساب'),
                        ),
                        const SizedBox(height: 10),
                        const Divider(height: 1),
                        const SizedBox(height: 12),
                        Directionality(
                          textDirection: TextDirection.rtl,
                          child: Row(
                            children: [
                              Expanded(
                                child: OutlinedButton.icon(
                                  onPressed: () => _openWhatsApp(context),
                                  icon: const Icon(
                                    Icons.chat_bubble_outline_rounded,
                                    size: 20,
                                  ),
                                  label: const Text('تواصل معنا'),
                                  style: OutlinedButton.styleFrom(
                                    foregroundColor:
                                        const Color(0xFF1F8A3B),
                                    minimumSize: const Size.fromHeight(48),
                                    side: BorderSide(
                                      color: const Color(0xFF1F8A3B)
                                          .withValues(alpha: 0.55),
                                    ),
                                  ),
                                ),
                              ),
                              const SizedBox(width: 10),
                              Expanded(
                                child: OutlinedButton.icon(
                                  onPressed: () => _openPhoneDialer(context),
                                  icon: const Icon(
                                    Icons.phone_outlined,
                                    size: 20,
                                  ),
                                  label: const Text('اتصل بنا'),
                                  style: OutlinedButton.styleFrom(
                                    minimumSize: const Size.fromHeight(48),
                                  ),
                                ),
                              ),
                            ],
                          ),
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
    );
  }

  Future<void> _submit() async {
    final phone = _phoneController.text.trim();
    final password = _passwordController.text;

    if (phone.isEmpty || password.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('أدخل رقم الهاتف وكلمة المرور.')),
      );
      return;
    }

    await ref
        .read(authControllerProvider.notifier)
        .login(phone, password, rememberMe: _rememberMe);
  }

  Future<void> _openWhatsApp(BuildContext context) async {
    await _launchExternalUri(
      context,
      whatsappChatUri,
      'تعذر فتح واتساب.',
    );
  }

  Future<void> _openPhoneDialer(BuildContext context) async {
    await _launchExternalUri(
      context,
      phoneDialUri,
      'تعذر فتح تطبيق الاتصال.',
    );
  }

  Future<void> _launchExternalUri(
    BuildContext context,
    Uri uri,
    String errorMessage,
  ) async {
    try {
      final opened = await launchUrl(
        uri,
        mode: LaunchMode.externalApplication,
      );
      if (!opened && context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(errorMessage)),
        );
      }
    } catch (_) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(errorMessage)),
        );
      }
    }
  }
}
