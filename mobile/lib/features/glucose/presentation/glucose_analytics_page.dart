import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/glucose_models.dart';
import '../data/glucose_repository.dart';
import '../../home/presentation/patient_dashboard_controller.dart';

final analyticsReadingsProvider=FutureProvider.autoDispose((ref)=>ref.watch(glucoseRepositoryProvider).getRecentReadings(days:30));

class GlucoseAnalyticsPage extends ConsumerWidget{
 const GlucoseAnalyticsPage({super.key});
 @override Widget build(BuildContext context,WidgetRef ref){
  final data=ref.watch(analyticsReadingsProvider);
  return Scaffold(appBar:AppBar(title:const Text('تحليل قراءات السكر')),body:data.when(
   loading:()=>const Center(child:CircularProgressIndicator()),
   error:(_,_)=>Center(child:FilledButton(onPressed:()=>ref.invalidate(analyticsReadingsProvider),child:const Text('إعادة المحاولة'))),
   data:(items)=>items.isEmpty?const Center(child:Text('لا توجد بيانات كافية للتحليل.')):ListView(padding:const EdgeInsets.all(20),children:[
    Card(child:ListTile(title:const Text('آخر 30 يومًا'),subtitle:Text('عدد القراءات: '+items.length.toString()),trailing:Text(_trend(items)))),
    const SizedBox(height:12),SizedBox(height:240,child:Card(child:Padding(padding:const EdgeInsets.all(16),child:CustomPaint(painter:_GlucosePainter(items),child:const SizedBox.expand())))),
    const SizedBox(height:12),Card(child:Padding(padding:const EdgeInsets.all(16),child:Text('يعرض هذا الرسم الاتجاه العام للقراءات المسجلة. النتائج مؤشرات برمجية للمساعدة على المتابعة وليست تشخيصًا طبيًا.'))))
   ]));
 }
 String _trend(List<GlucoseReading> x){if(x.length<2)return 'بيانات غير كافية';final a=x.take(x.length~/2).fold<double>(0,(s,e)=>s+e.glucoseValue)/((x.length~/2).clamp(1,999));final b=x.skip(x.length~/2).fold<double>(0,(s,e)=>s+e.glucoseValue)/(x.length-(x.length~/2));if(b<a-10)return 'تحسن';if(b>a+10)return 'تراجع';return 'مستقر';}
}
class _GlucosePainter extends CustomPainter{_GlucosePainter(this.items);final List<GlucoseReading> items;@override void paint(Canvas canvas,Size size){final paint=Paint()..strokeWidth=3..style=PaintingStyle.stroke;final values=items.reversed.take(30).map((e)=>e.glucoseValue).toList();if(values.length<2)return;final min=values.reduce((a,b)=>a<b?a:b),max=values.reduce((a,b)=>a>b?a:b),range=(max-min).abs()<1?1:max-min;final path=Path();for(var i=0;i<values.length;i++){final x=i*(size.width/(values.length-1));final y=size.height-((values[i]-min)/range)*size.height;if(i==0)path.moveTo(x,y);else path.lineTo(x,y);}canvas.drawPath(path,paint);}@override bool shouldRepaint(covariant _GlucosePainter oldDelegate)=>oldDelegate.items!=items;}
