using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Parameters
{
    public class MedicalRecordSpecificationParameters
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;

        public int PageIndex { get; set; } = 1;
        private int _pageSize = DefaultPageSize;
        public int PageSize 
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }   

    }
}
