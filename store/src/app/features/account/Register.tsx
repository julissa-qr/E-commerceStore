import Avatar from '@mui/material/Avatar';
import TextField from '@mui/material/TextField';
import Grid from '@mui/material/Grid';
import Box from '@mui/material/Box';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import Typography from '@mui/material/Typography';
import Container from '@mui/material/Container';
import { Paper } from '@mui/material';
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { LoadingButton } from '@mui/lab';
import { useAppDispatch } from '../../store/configureStore';
import agent from '../../api/agent';


export default function Register() {

    //let navigate = useNavigate();
    const dispatch = useAppDispatch();
    const { register, handleSubmit, setError, formState: { isSubmitting, errors, isValid } } = useForm({
        mode: 'all'
    })

    function handleApiErrors(errors: any){
        //console.log(errors);
        if(errors) {
            errors.forEach((error: string) => {
                if(error.includes('Password')){
                    setError('password', {message: error})
                }else if (error.includes('Email')){
                    setError('email', {message: error})
                }else if (error.includes('Username')){
                    setError('username', {message:error})
                }
            });
        }
    }

    return (

        <Container component={Paper} maxWidth="sm" sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', p: 4 }}>

            <Avatar sx={{ m: 1, bgcolor: 'secondary.main' }}>
                <LockOutlinedIcon />
            </Avatar>
            <Typography component="h1" variant="h5">
                Register
            </Typography>
            <Box component="form"
                onSubmit={handleSubmit((data) =>
                    agent.Account.register(data).catch(error => handleApiErrors(error)))
                } noValidate sx={{ mt: 1 }}>
                <TextField
                    margin="normal"
                    fullWidth
                    label="Username"
                    autoFocus
                    {...register('username', { required: 'Username is required' })}
                    error={!!errors.username}
                    required
                   // required
                   //helperText={errors?.username?.message}
                   //helperText={errors.username ? errors.message : ''}
                />

                <TextField
                    margin="normal"
                    fullWidth
                    label="Email"
                    {...register('email', { required: 'Email is required' })}
                    error={!!errors.email}
                    required
                />

                <TextField
                    margin="normal"
                    fullWidth
                    label="Password"
                    type="password"
                    {...register('password', { required: 'Password is required' })}
                    error={!!errors.password}
                    required
                //helperText={errors?.password?.message}
                />
        
                <LoadingButton
                    loading={isSubmitting}
                    disabled={!isValid}
                    type="submit"
                    fullWidth
                    variant="contained"
                    sx={{ mt: 3, mb: 2 }}
                >
                    Register
                </LoadingButton>
                <Grid container>
                    <Grid item>
                        <Link to='/login'>
                            {"Already have an account? Sign In!"}
                        </Link>
                    </Grid>
                </Grid>
            </Box>


        </Container>

    );
}