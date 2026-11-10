using Domain.Enums;

namespace Domain.Rules;

public static class GlucoseValueConverter
{
    public static decimal ToMgDl(decimal value, GlucoseUnit unit) => unit switch
    {
        GlucoseUnit.MgDl => value,
        GlucoseUnit.MmolL => value * 18m,
        _ => throw new ArgumentException("وحدة قياس السكر غير مدعومة.")
    };
}