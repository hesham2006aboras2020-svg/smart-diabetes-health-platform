import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../home/presentation/patient_dashboard_controller.dart';

class AddGlucosePage extends ConsumerStatefulWidget {
  const AddGlucosePage({super.key});

  @override
  ConsumerState<AddGlucosePage> createState() => _AddGlucosePageState();
}

class _AddGlucosePageState extends ConsumerState<AddGlucosePage> {
  final _formKey = GlobalKey<FormState>();
  final _valueController = TextEditingController();
  final _notesController = TextEditingController();
  String _unit = 'MgDl';
  String _context = 'Random';
  DateTime _measurementDateTime = DateTime.now();
  bool _saving = false;

  @override
  void dispose() {
    _valueController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;

    final parsed = double.tryParse(_valueController.text.trim());
    if (parsed == null) return;

    if (_measurementDateTime.isAfter(
      DateTime.now().add(const Duration(minutes: 5)),
    )) {
      _showMessage('لا يمكن تسجيل قراءة في المستقبل.');
      return;
    }

    setState(() => _saving = true);
    try {
      await ref.read(glucoseRepositoryProvider).addReading(
        glucoseValue: parsed,
        unit: _unit,
        measurementDateTime: _measurementDateTime,
        measurementContext: _context,
        notes: _notesController.text,
      );
      if (!mounted) return;
      ref.invalidate(patientDashboardProvider);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('تم حفظ قراءة السكر بنجاح.')),
      );
      Navigator.of(context).pop();
    } catch (_) {
      if (!mounted) return;
      _showMessage('تعذر حفظ القراءة. تحقق من البيانات وحاول مرة أخرى.');
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final unitLabel = _unit == 'MgDl' ? 'mg/dL' : 'mmol/L';
    return Scaffold(
      appBar: AppBar(title: const Text('تسجيل قراءة السكر')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            TextFormField(
              controller: _valueController,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: InputDecoration(
                labelText: 'قراءة السكر',
                suffixText: unitLabel,
                border: const OutlineInputBorder(),
              ),
              validator: (text) {
                final number = double.tryParse(text?.trim() ?? '');
                if (number == null || number <= 0) {
                  return 'أدخل قراءة صحيحة.';
                }
                if (_unit == 'MgDl' && number > 1000) {
                  return 'القراءة يجب ألا تتجاوز 1000 mg/dL.';
                }
                if (_unit == 'MmolL' && number > 55.6) {
                  return 'القراءة تتجاوز الحد المسموح.';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              initialValue: _unit,
              decoration: const InputDecoration(
                labelText: 'وحدة القياس',
                border: OutlineInputBorder(),
              ),
              items: const [
                DropdownMenuItem(value: 'MgDl', child: Text('mg/dL')),
                DropdownMenuItem(value: 'MmolL', child: Text('mmol/L')),
              ],
              onChanged: _saving ? null : (selected) {
                if (selected != null) setState(() => _unit = selected);
              },
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              initialValue: _context,
              decoration: const InputDecoration(
                labelText: 'سياق القياس',
                border: OutlineInputBorder(),
              ),
              items: const [
                DropdownMenuItem(value: 'Fasting', child: Text('صائم')),
                DropdownMenuItem(value: 'BeforeMeal', child: Text('قبل الوجبة')),
                DropdownMenuItem(value: 'AfterMeal', child: Text('بعد الوجبة')),
                DropdownMenuItem(value: 'BeforeSleep', child: Text('قبل النوم')),
                DropdownMenuItem(value: 'Random', child: Text('عشوائي')),
                DropdownMenuItem(value: 'Other', child: Text('أخرى')),
              ],
              onChanged: _saving ? null : (selected) {
                if (selected != null) setState(() => _context = selected);
              },
            ),
            const SizedBox(height: 16),
            Card(
              child: ListTile(
                title: const Text('وقت القياس'),
                subtitle: Text(_formatDateTime(_measurementDateTime)),
                trailing: const Icon(Icons.calendar_today),
                onTap: _saving ? null : _pickDateTime,
              ),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _notesController,
              maxLength: 500,
              maxLines: 3,
              decoration: const InputDecoration(
                labelText: 'ملاحظات (اختياري)',
                hintText: 'مثال: بعد وجبة خفيفة',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 24),
            FilledButton(
              onPressed: _saving ? null : _save,
              child: _saving
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(),
                    )
                  : const Text('حفظ القراءة'),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _pickDateTime() async {
    final date = await showDatePicker(
      context: context,
      initialDate: _measurementDateTime,
      firstDate: DateTime(2020),
      lastDate: DateTime.now().add(const Duration(minutes: 5)),
    );
    if (date == null || !mounted) return;

    final time = await showTimePicker(
      context: context,
      initialTime: TimeOfDay.fromDateTime(_measurementDateTime),
    );
    if (time == null) return;

    setState(() {
      _measurementDateTime = DateTime(
        date.year,
        date.month,
        date.day,
        time.hour,
        time.minute,
      );
    });
  }

  String _formatDateTime(DateTime dateTime) {
    return '${dateTime.day}/${dateTime.month}/${dateTime.year} '
        '${dateTime.hour.toString().padLeft(2, '0')}:'
        '${dateTime.minute.toString().padLeft(2, '0')}';
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message)),
    );
  }
}
