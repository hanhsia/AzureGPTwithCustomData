import { useState, useCallback, useEffect } from 'react';
import { AnonymousCredential, BlockBlobClient, newPipeline } from "@azure/storage-blob";
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import { Button, FormControl, FormHelperText, Typography, Box, List, ListItem, ListItemAvatar, Avatar, ListItemButton, IconButton } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import DownloadIcon from '@mui/icons-material/Download';
import ArticleIcon from '@mui/icons-material/Article';
import { useDropzone } from 'react-dropzone';
import FileListItem from './FileListItem';
import AuthService from "../services/auth.service";
import StorageService from "../services/storage.service"; 


interface FileState {
    file: File;
    status: string;
}

interface FilesUploaderProps {
    type: number;
}

const FilesUploader = (props: FilesUploaderProps) => {
    const { type } = props;
    const [fileStates, setFileStates] = useState<FileState[]>([]);

    const [fileList, setFileList] = useState<any>(null);
    const pipeline = newPipeline(new AnonymousCredential());
    

    const onChange = useCallback((acceptedFiles: File[]) => {

        setFileStates(prevFileStates => {
            const newFileStates = [...prevFileStates];
            for (let file of acceptedFiles) {
                newFileStates.push({ file: file, status: "0" });
            }
            return newFileStates;
        });

    }, [setFileStates])

    const showFiles = useCallback(async () => {
            let accessToken = await AuthService.getAccessToken("");
            let list: any;
            if (accessToken != null) {
                    list = await StorageService.getBlobList(accessToken, "");
                    if (list != null) {
                        setFileList(list);
                    }
                
            }
        
    }, [fileStates])

    const uploadPdf = useCallback(async () => {
        try {
            
                    //get access token
                    let file = fileStates[0].file;
                    let accessToken = await AuthService.getAccessToken("");

                    if (accessToken != null) {

                        const formData: FormData = new FormData();
                        formData.append('file', file);
                        // Add check for fileName
                        if (/^[a-zA-Z-_0-9. ]+$/.test(file.name)) {
                            await StorageService.uploadPdf(accessToken, formData);
                            removeFromList(0);
                        } else {
                            alert('The file name contains Chinese characters. Please rename it before uploading.')
                            removeFromList(0);
                        }

                    }
             
        } catch (err: any) {
            console.log(err);
        }
    }, [fileStates])

    useEffect(() => {

        if (fileStates.length > 0) {
            if (fileStates[0].status === "0") {
                setFileStates(prevFileStates => {
                    const newFileStates = prevFileStates;
                    newFileStates[0].status = "1";
                    return newFileStates;
                });
                uploadPdf();
            }
        }
        showFiles();
    }, [fileStates]);

    const deleteBlob = useCallback(async (name: string) => {
        
            let accessToken = await AuthService.getAccessToken("");

            if (accessToken != null) {
                let fileName = "";
                fileName = `${name}`;
                
                let result = await StorageService.deleteBlob(accessToken, fileName);
                if (result) {
                    showFiles();
                }
            }
        

    }, []);

    const downloadBlob = useCallback(async (name: string) => {
        
            let accessToken = await AuthService.getAccessToken("");

            if (accessToken != null) {
                let fileName = "";
                fileName = `${name}`
                
                let response = await StorageService.downloadBlob(accessToken, fileName);
                if (response) {
                    const blob = await response.blob();
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = response.headers.get('Content-Disposition')?.split('filename=')[1] || 'downloaded_file';
                    a.click();
                    window.URL.revokeObjectURL(url);
                }
            }
        

    }, []);


    const removeFromList = useCallback((index: number) => {
        setFileStates(prevFileStates => {
            const newFileStates = [...prevFileStates];
            newFileStates.splice(index, 1);
            return newFileStates;
        });
    }, []);



    const { fileRejections, getRootProps, getInputProps, open } =
        useDropzone({
            onDropAccepted: onChange,
            noClick: true,
            noKeyboard: true,
            accept: type === 1 ? {
                'text/pdf': ['.pdf'],
            } : undefined 
        });

    return (
        <>
            <Box
                {...getRootProps()}
                sx={{
                    border: 1,
                    borderRadius: 1,
                    borderColor: 'rgba(0, 0, 0, 0.23)',
                    paddingY: 3,
                    paddingX: 1,
                    '&:hover': {
                        borderColor: 'text.primary',
                    },
                    '&:focus-within': {
                        borderColor: 'primary.main',
                        borderWidth: 2,
                    }
                }}>
                <FormControl
                    sx={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center' }}>
                    <input {...getInputProps()} />
                    <CloudUploadIcon sx={{ fontSize: 40 }} color={'primary'} />
                    <Typography variant="caption" textAlign="center" sx={{ paddingY: 1 }}>
                        Drag and drop files here, or click to select files
                    </Typography>
                    <Button variant="contained" onClick={open} sx={{ marginBottom: 1 }}>
                        Upload
                    </Button>
                    <FormHelperText> {fileRejections[0]?.errors[0]?.message} </FormHelperText>
                </FormControl>
                <Box
                    component="ul"
                    sx={{
                        display: 'flex',
                        justifyContent: 'center',
                        flexWrap: 'wrap',
                        listStyle: 'none',
                        p: 0.5,
                        m: 0,
                    }}>
                    {fileStates?.map((fileState, i) =>
                        <FileListItem key={"fileListItem-" + fileState.file.name} name={fileState.file.name} disabled={fileState.status !== "0"} onDelete={() => removeFromList(i)} />
                    )}

                </Box>

            </Box>
            <Box>
                <List>
                    {fileList?.map((file: any, i: number) =>
                        <ListItem key={"listItem-" + file.name}

                            secondaryAction={
                                <>
                                    <IconButton edge="end" aria-label="download" onClick={() => downloadBlob(file.name)}>
                                        <DownloadIcon />
                                    </IconButton>
                                    <IconButton edge="end" aria-label="delete" onClick={() => deleteBlob(file.name)}>
                                        <DeleteIcon />
                                    </IconButton>
                                </>}
                        >
                            <ListItemAvatar>
                                <Avatar>
                                    <ArticleIcon />
                                </Avatar>
                            </ListItemAvatar>
                            <ListItemButton >
                                {file.name}
                            </ListItemButton>
                        </ListItem>
                    )}
                </List>
            </Box>
        </>

    );
};

export default FilesUploader;