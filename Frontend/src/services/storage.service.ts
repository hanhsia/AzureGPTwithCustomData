import axios from "axios";

const API_URL = process.env.REACT_APP_DOTNET_API_PATH;

const getBlobUploadUri = async (token: string, name: string): Promise<string | null> => {
    try {

        let response = await axios.get(
            API_URL + `/api/file/getuploaduri?filepath=${name}`

        )
        if (response.status === 200) {
            if (response.data.isSuccess) {
                return response.data.content.uri;
            }
        }
    }
    catch (error) {
        console.log(error);
    }

    return null;
};

const getBlobDownloadUri = async (token: string, name: string): Promise<string | null> => {
    try {

        let response = await axios.get(
            API_URL + `/api/file/getdownloaduri?filepath=${name}`
        )
        if (response.status === 200) {
            if (response.data.isSuccess) {
                return response.data.content.uri;
            }
        }
    }
    catch (error) {
        console.log(error);
    }

    return null;
};


const uploadPdf = async (token: string, formData: FormData): Promise<boolean> => {

    let response = await axios.post(
        API_URL + "/api/file/uploadPdf",
        formData,
        {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        }
    )
    if (response.status === 200) {
        return true;
    }
    return false
}

const getBlobList = async (token: string, prefix: string): Promise<any | null> => {
    try {

        let response = await axios.post(
            API_URL + "/api/file/list",
            {
                prefix
            }
        )
        if (response.status === 200) {
            if (response.data.isSuccess) {
                return response.data.content;
            }
        }
    }
    catch (error) {
        console.log(error);
    }

    return null;
};


const deleteBlob = async (token: string, name: string): Promise<boolean> => {
    try {

        let response = await axios.post(
            API_URL + "/api/file/delete",
            {
                name
            }
        )
        if (response.status === 200) {
            if (response.data.isSuccess) {
                return true;
            }
        }
    }
    catch (error) {
        console.log(error);
    }

    return false;
}


const downloadBlob = async (token: string, name: string): Promise<any> => {
    try {

        const response = await fetch(API_URL + `/api/file/download?filePath=${encodeURIComponent(name)}`);

        if (response.ok) {

            return response;
        }
        else {
            console.error('Download failed:', response.statusText);
        }
    }
    catch (error) {
        console.log(error);
    }

    return false;
};

const StorageService = {
    getBlobUploadUri,
    getBlobDownloadUri,
    downloadBlob,
    getBlobList,
    deleteBlob,
    uploadPdf
}
export default StorageService;