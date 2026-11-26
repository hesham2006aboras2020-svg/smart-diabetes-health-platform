import 'package:flutter_test/flutter_test.dart';
import 'package:smart_diabetes_mobile/features/auth/presentation/login_page.dart';

void main() {
  test('WhatsApp action uses the correct contact destination', () {
    expect(whatsappChatUri.toString(), 'https://wa.me/967739303311');
  });

  test('phone action uses the correct dial destination', () {
    expect(phoneDialDestination, '+697772106560');
    expect(phoneDialUri.toString(), 'tel:+697772106560');
  });
}
