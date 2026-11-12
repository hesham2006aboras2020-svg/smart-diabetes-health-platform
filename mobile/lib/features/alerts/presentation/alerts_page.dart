import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/alert_repository.dart';
import '../../auth/presentation/auth_controller.dart';
final alertRepositoryProvider=Provider((ref)=>AlertRepository(ref.watch(apiClientProvider)));
final alertsProvider=FutureProvider.autoDispose((ref)=>ref.watch(alertRepositoryProvider).all());
class AlertsPage extends ConsumerWidget{
 const AlertsPage({super.key});
 @override Widget build(BuildContext context,WidgetRef ref){
  final state=ref.watch(alertsProvider);
  return Scaffold(appBar:AppBar(title:const Text('التنبيهات')),body:state.when(
   loading:()=>const Center(child:CircularProgressIndicator()),
   error:(_,_)=>Center(child:FilledButton(onPressed:()=>ref.invalidate(alertsProvider),child:const Text('إعادة المحاولة'))),
   data:(items){
    if(items.isEmpty)return const Center(child:Text('لا توجد تنبيهات.'));
    return ListView.separated(itemCount:items.length,separatorBuilder:(_,_)=>const Divider(),itemBuilder:(_,i){
     final item=items[i];
     return ListTile(title:Text(item.message),subtitle:Text(item.createdAt.toString()),trailing:item.read?null:const Icon(Icons.warning_amber),onTap:()async{if(!item.read){await ref.read(alertRepositoryProvider).read(item.id);ref.invalidate(alertsProvider);}});
    });
   },
  ));
 }
}