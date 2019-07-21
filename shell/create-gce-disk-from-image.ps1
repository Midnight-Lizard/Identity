gcloud compute disks create ml-small-disk-2 `
    --project=midnight-lizard-2019-fall `
    --zone=us-central1-a `
    --type=pd-standard `
    --size=10GB `
    --image-project=midnight-lizard-2019 `
    --image=ml-small-disk-image