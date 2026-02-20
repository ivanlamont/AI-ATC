using System.Diagnostics;
using AIATC.Data.Types.Common;

namespace AIATC.Data.Models.Types;

using Microsoft.EntityFrameworkCore;

/**<summary>
Various courses and bearings according to the specification.
</summary>
<remarks>See section 5.26, 5.28, 5.47, 5.58, 5.62 and 5.167.</remarks>*/
// ARINC mapping attributes removed
[DebuggerDisplay($"{{{nameof(Value)}}}, {{{nameof(Type)}}}")]
[Owned]
public class Course
{
    /// <summary>Angle.</summary>
    /// <value>Degrees.</value>
    public float Value { get; set; }

    /// <inheritdoc cref="CourseType"/>
    public CourseType Type { get; set; }

    public Course(float Value, CourseType Type)
    {
        this.Value = Value;
        this.Type = Type;
    }
}
