import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'features/auth/presentation/auth_controller.dart';
import 'features/auth/presentation/login_page.dart';
import 'features/home/presentation/home_page.dart';

class DiabetesApp extends ConsumerWidget {
  const DiabetesApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final auth = ref.watch(authControllerProvider);

    final router = GoRouter(
      initialLocation: '/login',
      redirect: (context, state) {
        final isLogin = state.matchedLocation == '/login';
        if (auth.status == AuthStatus.loading) return null;
        if (auth.status == AuthStatus.authenticated && isLogin) {
          return '/home';
        }
        if (auth.status != AuthStatus.authenticated && !isLogin) {
          return '/login';
        }
        return null;
      },
      routes: [
        GoRoute(
          path: '/login',
          builder: (context, state) => const LoginPage(),
        ),
        GoRoute(
          path: '/home',
          builder: (context, state) => const HomePage(),
        ),
      ],
    );

    return MaterialApp.router(
      title: 'منصة السكري الذكية',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorSchemeSeed: const Color(0xFF157A8A),
        scaffoldBackgroundColor: const Color(0xFFF7FAFA),
        inputDecorationTheme: const InputDecorationTheme(
          border: OutlineInputBorder(),
        ),
      ),
      locale: const Locale('ar'),
      supportedLocales: const [Locale('ar'), Locale('en')],
      routerConfig: router,
    );
  }
}
