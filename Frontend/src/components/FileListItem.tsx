import UploadFileIcon from '@mui/icons-material/UploadFile';
import { Chip } from '@mui/material';
import { styled } from '@mui/material/styles';

export interface FileListItemProps {
    name: string;
    disabled: boolean | undefined;
    onDelete: () => void;
}

const ListItem = styled('li')(({ theme }) => ({
    margin: theme.spacing(0.5),
}));

const FileListItem = ({ name, disabled, onDelete }: FileListItemProps) => (
    <ListItem key={"listItem-"+name}>
        <Chip label={name} icon={<UploadFileIcon />} variant="outlined" sx={{ maxWidth: 200 }} onDelete={onDelete} disabled={disabled} />
    </ListItem>
);

export default FileListItem;