﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Wia;
using NAPS2.Util;

namespace NAPS2.Worker
{
    public class GrpcWorkerServiceAdapter
    {
        private GrpcWorkerService.GrpcWorkerServiceClient client;

        public GrpcWorkerServiceAdapter(int port)
        {
            this.client = new GrpcWorkerService.GrpcWorkerServiceClient(new Channel("localhost", port, ChannelCredentials.Insecure));
        }
        public void Init(string recoveryFolderPath)
        {
            var req = new InitRequest { RecoveryFolderPath = recoveryFolderPath };
            var resp = client.Init(req);
            GrpcHelper.HandleErrors(resp.Error);
        }

        public WiaConfiguration Wia10NativeUI(string scanDevice, IntPtr hwnd)
        {
            var req = new Wia10NativeUiRequest
            {
                DeviceId = scanDevice,
                Hwnd = (ulong)hwnd
            };
            var resp = client.Wia10NativeUi(req);
            GrpcHelper.HandleErrors(resp.Error);
            return resp.WiaConfigurationXml.FromXml<WiaConfiguration>();
        }

        public List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl)
        {
            var req = new TwainGetDeviceListRequest { TwainImpl = twainImpl.ToXml() };
            var resp = client.TwainGetDeviceList(req);
            GrpcHelper.HandleErrors(resp.Error);
            return resp.DeviceListXml.FromXml<List<ScanDevice>>();
        }

        public async Task TwainScan(ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd, CancellationToken cancelToken, Action<ScannedImage, string> imageCallback)
        {
            var req = new TwainScanRequest
            {
                ScanDeviceXml = scanDevice.ToXml(),
                ScanProfileXml = scanProfile.ToXml(),
                ScanParamsXml = scanParams.ToXml(),
                Hwnd = (ulong)hwnd
            };
            var streamingCall = client.TwainScan(req, cancellationToken: cancelToken);
            while (await streamingCall.ResponseStream.MoveNext())
            {
                var resp = streamingCall.ResponseStream.Current;
                GrpcHelper.HandleErrors(resp.Error);
                var storage = new OwnedTransferStorage(resp.FilePath);
                var metadata = StorageManager.ImageMetadataFactory.CreateMetadata(storage);
                metadata.Deserialize(resp.MetadataXml);
                var scannedImage = new ScannedImage(storage, metadata, new StorageConvertParams());
                var thumbnail = resp.Thumbnail.ToByteArray();
                if (thumbnail.Length > 0)
                {
                    var thumbnailStorage = new MemoryStreamStorage(new MemoryStream(thumbnail));
                    scannedImage.SetThumbnail(StorageManager.ConvertToImage(thumbnailStorage, new StorageConvertParams()));
                }
                imageCallback?.Invoke(scannedImage, resp.RenderedFilePath);
            }
        }

        public MapiSendMailReturnCode SendMapiEmail(EmailMessage message)
        {
            var req = new SendMapiEmailRequest { EmailMessageXml = message.ToXml() };
            var resp = client.SendMapiEmail(req);
            GrpcHelper.HandleErrors(resp.Error);
            return resp.ReturnCodeXml.FromXml<MapiSendMailReturnCode>();
        }

        public byte[] RenderThumbnail(ScannedImage.Snapshot snapshot, int size)
        {
            var req = new RenderThumbnailRequest
            {
                SnapshotXml = snapshot.ToXml(),
                Size = size
            };
            var resp = client.RenderThumbnail(req);
            GrpcHelper.HandleErrors(resp.Error);
            return resp.Thumbnail.ToByteArray();
        }
    }
}