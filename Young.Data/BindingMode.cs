using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data
{
    public class BindingMode
    {
        public DataType DataMode {get;set;}

        public LoopType LoopMode {get;set;}

        public RecusionType RecusionMode {get;set;}

        public SettingType SettingMode {get;set;}

        public bool IsUsingSampleData {get;set;}
       
    }

    public enum DataType
    {
        FromShareTable = 0,
        FromPrivateTable = 1,
    }

    public enum LoopType
    {
         NoLoop = 0,
         Loop = 1,
    }

    public enum RecusionType
    {
        NoRecursion = 0,
        Recusion =1,
    }

    public enum SettingType
    {
        MethodAndProperty = 0,
        PropertyOnly = 1,
        MethodOnly = 2,
        
    }
}
