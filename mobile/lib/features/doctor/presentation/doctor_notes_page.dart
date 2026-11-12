import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/presentation/auth_controller.dart';

class DoctorNotesPage extends ConsumerStatefulWidget {
  const DoctorNotesPage(this.patientId, {super.key});
  final String patientId;
  @override
  ConsumerState<DoctorNotesPage> createState() => _DoctorNotesState();
}
class _DoctorNotesState extends ConsumerState<DoctorNotesPage> {
  final note = TextEditingController();
  bool saving = false;
  @override void dispose(){note.dispose();super.dispose();}
  @override Widget build(BuildContext context){
    return Scaffold(appBar:AppBar(title:const Text('ملاحظات الطبيب')),body:Padding(padding:const EdgeInsets.all(20),child:Column(children:[
      TextField(controller:note,maxLines:6,decoration:const InputDecoration(labelText:'ملاحظة المتابعة')),
      const SizedBox(height:12),
      FilledButton(onPressed:saving?null:()=>_save(context),child:Text(saving?'جارٍ الحفظ...':'حفظ الملاحظة')),
    ])));
  }
  Future<void> _save(BuildContext context) async {
    if(note.text.trim().isEmpty)return;
    setState(()=>saving=true);
    try{
      await ref.read(apiClientProvider).dio.post('/api/doctor-notes',data:{'patientId':widget.patientId,'note':note.text.trim()});
      note.clear();
      if(context.mounted)ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content:Text('تم حفظ الملاحظة.')));
    } finally { if(mounted)setState(()=>saving=false); }
  }
}