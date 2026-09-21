namespace UnityEngine {
 public class ScriptableObject { public string name; }
 public class SerializeField : System.Attribute {}
 public class RangeAttribute : System.Attribute { public RangeAttribute(int a,int b){} }
 public class MinAttribute : System.Attribute { public MinAttribute(int a){} }
 public class TextAreaAttribute : System.Attribute {}
 public class TooltipAttribute : System.Attribute { public TooltipAttribute(string a){} }
 public class CreateAssetMenuAttribute : System.Attribute { public string fileName,menuName; }
}
namespace Shift.Game {
 public class LevelDesign {}
 public class LevelDesignCatalog { public static LevelDesignCatalog Current => null; public LevelDesign Find(LevelData l)=>null; }
}
