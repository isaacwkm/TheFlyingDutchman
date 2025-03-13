using System.Reflection;

public class PropertyReference
{
    private object instance;
    private PropertyInfo property;

    public PropertyReference(object instance, string key)
    {
        this.instance = instance;
        this.property = instance.GetType().GetProperty(key);
    }

    public object value
    {
        get
        {
            return property.GetValue(instance);
        }
        set
        {
            property.SetValue(instance, value);
        }
    }
}
