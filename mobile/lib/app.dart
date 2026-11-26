import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'features/auth/presentation/auth_controller.dart';
import 'features/auth/presentation/login_page.dart';
import 'features/auth/presentation/register_page.dart';
import 'features/home/presentation/home_page.dart';
import 'features/web/presentation/web_landing_page.dart';

class DiabetesApp extends ConsumerStatefulWidget {
  const DiabetesApp({super.key});

  @override
  ConsumerState<DiabetesApp> createState() => _DiabetesAppState();
}

class _DiabetesAppState extends ConsumerState<DiabetesApp> {
  late GoRouter _router;
  late AuthStatus _authStatus;
  final ValueNotifier<int> _routerRefresh = ValueNotifier<int>(0);

  @override
  void initState() {
    super.initState();
    final auth = ref.read(authControllerProvider);
    _authStatus = auth.status;

    _router = GoRouter(
      initialLocation: '/',
      refreshListenable: _routerRefresh,
      redirect: (context, state) {
        final isPublicRoute = state.matchedLocation == '/' ||
            state.matchedLocation == '/login' ||
            state.matchedLocation == '/register';

        if (_authStatus == AuthStatus.loading) return null;
        if (_authStatus == AuthStatus.authenticated && isPublicRoute) {
          return '/home';
        }
        if (_authStatus != AuthStatus.authenticated && !isPublicRoute) {
          return '/login';
        }
        return null;
      },
      routes: [
        GoRoute(
          path: '/',
          builder: (context, state) => const WebLandingPage(),
        ),
        GoRoute(
          path: '/login',
          builder: (context, state) => const LoginPage(),
        ),
        GoRoute(
          path: '/register',
          builder: (context, state) => const RegisterPage(),
        ),
        GoRoute(
          path: '/home',
          builder: (context, state) => const HomePage(),
        ),
      ],
    );

    ref.listenManual<AuthState>(authControllerProvider, (_, next) {
      _authStatus = next.status;
      _routerRefresh.value++;
    });
  }

  @override
  void dispose() {
    _router.dispose();
    _routerRefresh.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'منصة السكري الذكية',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF45C6B7),
          brightness: Brightness.dark,
          surface: const Color(0xFF132426),
        ),
        scaffoldBackgroundColor: const Color(0xFF0B1719),
        fontFamily: 'Arial',
        visualDensity: VisualDensity.standard,
        textTheme: ThemeData.dark().textTheme.apply(fontFamily: 'Arial'),
        appBarTheme: const AppBarTheme(
          centerTitle: false,
          elevation: 0,
          scrolledUnderElevation: 0,
          backgroundColor: Colors.transparent,
          surfaceTintColor: Colors.transparent,
          titleTextStyle: TextStyle(
            fontWeight: FontWeight.w900,
            fontSize: 20,
            color: Color(0xFFF3FBFA),
          ),
        ),
        cardTheme: CardThemeData(
          elevation: 1,
          margin: EdgeInsets.zero,
          color: const Color(0xFF192B2D),
          shadowColor: const Color(0x66000000),
          surfaceTintColor: Colors.transparent,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.all(Radius.circular(24)),
            side: BorderSide(color: Color(0x2A45C6B7)),
          ),
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: const Color(0xFF142729),
          isDense: true,
          contentPadding: const EdgeInsets.symmetric(horizontal: 17, vertical: 16),
          prefixIconColor: const Color(0xFF45C6B7),
          suffixIconColor: const Color(0xFF9FB8B5),
          floatingLabelStyle: const TextStyle(fontWeight: FontWeight.w700),
          border: OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(17)),
            borderSide: BorderSide(color: Color(0x3A75A29C)),
          ),
          enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(17)),
            borderSide: BorderSide(color: Color(0x3A75A29C)),
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(17)),
            borderSide: BorderSide(color: Color(0xFF45C6B7), width: 1.6),
          ),
          errorBorder: OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(17)),
            borderSide: BorderSide(color: Color(0xFFBA1A1A)),
          ),
          focusedErrorBorder: OutlineInputBorder(
            borderRadius: BorderRadius.all(Radius.circular(17)),
            borderSide: BorderSide(color: Color(0xFFBA1A1A), width: 1.6),
          ),
        ),
        filledButtonTheme: FilledButtonThemeData(
          style: FilledButton.styleFrom(
            minimumSize: const Size.fromHeight(50),
            padding: const EdgeInsets.symmetric(horizontal: 22, vertical: 14),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(17),
            ),
            textStyle: const TextStyle(fontWeight: FontWeight.w800),
          ),
        ),
        outlinedButtonTheme: OutlinedButtonThemeData(
          style: OutlinedButton.styleFrom(
            minimumSize: const Size.fromHeight(48),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(17),
            ),
            side: const BorderSide(color: Color(0x5575A29C)),
          ),
        ),
        chipTheme: ChipThemeData(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14),
          ),
          side: BorderSide.none,
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        ),
        navigationBarTheme: NavigationBarThemeData(
          elevation: 2,
          height: 68,
          backgroundColor: const Color(0xFF102123),
          indicatorColor: const Color(0x3345C6B7),
          labelTextStyle: const WidgetStatePropertyAll(
            TextStyle(fontWeight: FontWeight.w800, fontSize: 12),
          ),
        ),
        dividerTheme: const DividerThemeData(
          space: 1,
          thickness: 1,
          color: Color(0x2A75A29C),
        ),
        snackBarTheme: SnackBarThemeData(
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          insetPadding: const EdgeInsets.all(16),
        ),
        dialogTheme: DialogThemeData(
          elevation: 0,
          backgroundColor: const Color(0xFF192B2D),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(24),
          ),
        ),
      ),
      locale: const Locale('ar'),
      supportedLocales: const [Locale('ar'), Locale('en')],
      localizationsDelegates: GlobalMaterialLocalizations.delegates,
      builder: (context, child) => Directionality(
        textDirection: TextDirection.rtl,
        child: child ?? const SizedBox.shrink(),
      ),
      routerConfig: _router,
    );
  }
}

