﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestEMVTransaction.emv
{
  /**
   * Tag type
   */
  public enum TagType
  {

    /**
     * A primitive data object where the value field contains a data element for
     * financial transaction interchange.
     */
    PRIMITIVE,
    /**
     * A constructed data object where the value field contains one or more
     * primitive or constructed data objects. The value field of a constructed
     * data object is called a template.
     */
    CONSTRUCTED
  }
}
