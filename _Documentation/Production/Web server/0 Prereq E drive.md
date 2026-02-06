# Create an E-drive on the server

This guide explains how to create an E-drive on a Windows Server 2019 or newer by shrinking the C drive to use the free space. The disk will be named "GO".

## Steps

1. **Open Disk Management**:
   - Press `Win + X` and select **Disk Management** from the menu.

2. **Shrink the C Drive**:
   - Right-click on the C drive and select **Shrink Volume**.
   - Enter the amount of space to shrink. Ensure that the C drive remains around 200 GB after shrinking.
   - Click **Shrink** to proceed.

3. **Initialize the Disk** (if necessary):
   - If the new unallocated space is not initialized, right-click on it and select **Initialize Disk**.
   - Choose **GPT (GUID Partition Table)** and click **OK**.

4. **Create a New Volume**:
   - Right-click on the unallocated space and select **New Simple Volume**.
   - Follow the wizard to specify the volume size and assign the drive letter **E**.

5. **Format the Volume**:
   - Choose **NTFS** as the file system.
   - Enter **GO** as the volume label.
   - Complete the wizard to create and format the new volume.

6. **Verify the Drive**:
   - Open **File Explorer** and verify that the new E-drive is listed with the name **GO**.

