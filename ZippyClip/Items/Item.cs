﻿#nullable enable

namespace ZippyClip.Items
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using ZippyClip.Actions;

    public abstract class Item: IEquatable<Item>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int? listIndex;

        public int? ListIndex
        {
            get => listIndex;
            set
            {
                listIndex = value;
                OnPropertyChanged("ListIndex");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Item? MakeFromClipboard()
        {
            if (Clipboard.ContainsText())
            {
                return Make(Clipboard.GetText());
            }

            if (Clipboard.ContainsImage())
            {
                // TODO: Catch exception:
                /*
                 * System.Runtime.InteropServices.COMException: "OpenClipboard fehlgeschlagen (Ausnahme von HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))"
                 */
                try
                {
                    return Make(Clipboard.GetImage());
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Console.Error.WriteLine(e.Message); // TODO: Log

                    return null;
                }
            }

            return null;
        }

        private static Item Make(string text)
        {
            if (Uri.TryCreate(text, UriKind.Absolute, out Uri uri))
            {
                return new UriItem(uri);
            }
            else
            {
                return new TextItem(text);
            }
        }

        private static Item Make(BitmapSource bitmapSource)
        {
            return new ImageItem(bitmapSource);
        }

        protected abstract void CopyContentsToClipboard(IDataObject data);

        public void CopyToClipboard()
        {
            var data = new DataObject();

            data.SetData(ClipboardNotification.ClipboardIgnoreFormat, 0);
            
            CopyContentsToClipboard(data);

            Clipboard.SetDataObject(data);
        }

        public override abstract int GetHashCode();

        public abstract bool Equals(Item other);

        public virtual BitmapSource? GetPreviewImage() => null;

        public virtual string? GetPreviewText() => null;

        public virtual bool SupportsPreview => false;

        public abstract void PerformAction(IActionPerformer actionPerformer);
    }
}
