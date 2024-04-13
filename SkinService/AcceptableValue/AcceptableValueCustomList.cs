﻿namespace SkinService.AcceptableValue
{
    /// <summary>
    ///     Can be Modify Acceptable Values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AcceptableValueCustomList<T> : BepInEx.Configuration.AcceptableValueList<T>
        where T : System.IEquatable<T>
    {
        [System.Obsolete("Used AcceptableValuesCustom", true)]
        public override T[] AcceptableValues => AcceptableValuesCustom;

        public T[] AcceptableValuesCustom
        {
            get => acceptableValuesCustom;
            set
            {
                if (acceptableValuesCustom == null)
                {
                    throw new System.ArgumentNullException(nameof(acceptableValuesCustom));
                }

                acceptableValuesCustom = value.Length > 0
                    ? value
                    : throw new System.ArgumentException("At least one acceptable value is needed",
                        nameof(acceptableValuesCustom));
            }
        }

        private T[] acceptableValuesCustom;

        public AcceptableValueCustomList(params T[] acceptableValues) : base(acceptableValues)
        {
            //Base constructor already checked
            acceptableValuesCustom = acceptableValues;
        }
    }
}