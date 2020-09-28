namespace FrontierDevelopments.Shields
{
    public interface ShieldSetting
    {
    }

    public abstract class BaseShieldSetting<T> : ShieldSetting
    {
        private readonly T _value;

        protected BaseShieldSetting(T value)
        {
            _value = value;
        }

        public T Get()
        {
            return _value;
        }
    }

    public class RadiusSetting : BaseShieldSetting<int>
    {
        public RadiusSetting(int value) : base(value)
        {
        }
    }

    public class RenderFieldSetting : BaseShieldSetting<bool>
    {
        public RenderFieldSetting(bool value) : base(value)
        {
        }
    }

    public class ThermalShutoffSetting : BaseShieldSetting<bool>
    {
        public ThermalShutoffSetting(bool value) : base(value)
        {
        }
    }
}