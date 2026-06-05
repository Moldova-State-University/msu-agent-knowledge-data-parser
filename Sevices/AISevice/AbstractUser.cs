using System;
using System.Collections.Generic;
using System.Text;

namespace LlamaParserV2.Sevices.AISevice;

abstract class AbstractUser
{
    public abstract string Role { get; }
    public abstract string Prompt { get;  }
}
