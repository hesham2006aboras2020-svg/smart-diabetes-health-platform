import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/admin_repository.dart';
import '../../auth/presentation/auth_controller.dart';
final adminRepositoryProvider=Provider((ref)=>AdminRepository(ref.watch(apiClientProvider)));
final adminSummaryProvider=FutureProvider.autoDispose((ref)=>ref.watch(adminRepositoryProvider).summary());
final approvalsProvider=FutureProvider.autoDispose((ref)=>ref.watch(adminRepositoryProvider).approvals());
class AdminDashboardPage extends ConsumerWidget{
 const AdminDashboardPage({super.key});
 @override Widget build(BuildContext context,WidgetRef ref){
  final s=ref.watch(adminSummaryProvider),a=ref.watch(approvalsProvider);
  return Scaffold(appBar:AppBar(title:const Text('لوحة الإدارة')),body:ListView(padding:const EdgeInsets.all(20),children:[
   s.when(loading:()=>const LinearProgressIndicator(),error:(_,_)=>const Text('تعذر تحميل الملخص.'),data:(x)=>Wrap(spacing:10,runSpacing:10,children:[_metric('المرضى',x.patients),_metric('الأطباء',x.doctors),_metric('طلبات معلقة',x.pending),_metric('تنبيهات غير مقروءة',x.alerts)])),
   const SizedBox(height:24),Text('طلبات اعتماد الأطباء',style:Theme.of(context).textTheme.titleLarge),const SizedBox(height:8),
   a.when(loading:()=>const CircularProgressIndicator(),error:(_,_)=>const Text('تعذر تحميل الطلبات.'),data:(items){
    if(items.isEmpty)return const Text('لا توجد طلبات معلقة.');
    return Column(children:[for(final x in items)Card(child:ListTile(
     title:Text(x.name),subtitle:Text(x.email+' • ترخيص: '+x.license),
     trailing:Row(mainAxisSize:MainAxisSize.min,children:[
      IconButton(onPressed:()async{await ref.read(adminRepositoryProvider).approve(x.id);ref.invalidate(approvalsProvider);ref.invalidate(adminSummaryProvider);},icon:const Icon(Icons.check)),
      IconButton(onPressed:()async{await ref.read(adminRepositoryProvider).reject(x.id,'تم الرفض من الإدارة');ref.invalidate(approvalsProvider);ref.invalidate(adminSummaryProvider);},icon:const Icon(Icons.close)),
     ]),
    ))]);
   }),
  ]));
 }
 Widget _metric(String label,int value)=>SizedBox(width:150,child:Card(child:Padding(padding:const EdgeInsets.all(16),child:Column(children:[Text(label),Text(value.toString(),style:const TextStyle(fontSize:24,fontWeight:FontWeight.bold))]))));
}