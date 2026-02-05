using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace SoftOne.Soe.Common.Util
{
    public class DynamicEntity : DynamicObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private readonly IDictionary<string, object> dictionary = new Dictionary<string, object>();

        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return dictionary.Keys;
        }

        public void AddDictionaryMember(string name, object value)
        {
            this[name] = value;
        }

        public void ForceNotification(string name)
        {
            OnPropertyChanged(name);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name.ToLower()];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value != null)
                this[binder.Name.ToLower()] = value;

            return true;
        }

        public object this[string columnName]
        {
            get
            {
                if (dictionary.ContainsKey(columnName.ToLower()))
                    return dictionary[columnName.ToLower()];

                return null;
            }
            set
            {
                if (!dictionary.ContainsKey(columnName.ToLower()))
                {
                    dictionary.Add(columnName.ToLower(), value);
                    OnPropertyChanged(columnName);
                }
                else
                {
                    if (dictionary[columnName.ToLower()] != value)
                    {
                        dictionary[columnName.ToLower()] = value;
                        OnPropertyChanged(columnName);
                    }
                }
            }
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StaffingNeedsDynamicRow : DynamicEntity, INotifyPropertyChanged
    {
        public bool IsSumRow { get; set; }

        public override void OnPropertyChanged(string propertyName)
        {
            if (IsSumRow)
            {
                if ((propertyName.ToLower().StartsWith("per") && propertyName.ToLower().EndsWith("code")) ||
                    (propertyName.ToLower().StartsWith("per") && propertyName.ToLower().EndsWith("minutes")) ||
                    (propertyName.ToLower().StartsWith("per") && propertyName.ToLower().EndsWith("value")) ||
                    propertyName.ToLower() == "sum")
                    base.OnPropertyChanged(propertyName);
            }
            else
            {
                if (propertyName.ToLower().StartsWith("per") && propertyName.ToLower().EndsWith("code"))
                    base.OnPropertyChanged(propertyName);
            }
        }
    }
}
