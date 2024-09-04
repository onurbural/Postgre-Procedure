using System;
using System.Collections.Generic;

namespace Postgre_Procedures.Models;

public partial class CategoryRelation
{
    public int ParentCategoryId { get; set; }

    public int ChildCategoryId { get; set; }
}
